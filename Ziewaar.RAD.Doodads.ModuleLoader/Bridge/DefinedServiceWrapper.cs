#nullable enable
using Ziewaar.RAD.Doodads.CoreLibrary;
using Ziewaar.RAD.Doodads.ModuleLoader.Exceptions;
using Ziewaar.RAD.Doodads.ModuleLoader.Profiler;

namespace Ziewaar.RAD.Doodads.ModuleLoader.Bridge;

public class DefinedServiceWrapper : IAmbiguousServiceWrapper
{
    private static readonly object NullBuster = new();
    
    private ServiceIdentity ServiceIdentity = new()
    {
        Typename = "Undefined", Filename = "Undeclared", Line = -1, Position = -1,
    };

    private Type? Type;
    private EventInfo? OnThenEventInfo, OnElseEventInfo;
    private CallForInteraction? DoneDelegate;
    public CursorText? Position { get; private set; }
    private SortedList<string, CallForInteraction>? EventHandlers;
    public string? TypeName { get; private set; }
    private List<Action>? CleanupPropagation;
    public IService? Instance { get; private set; }
    public StampedMap? Constants { get; private set; }
    public event CallForInteraction? DiagnosticOnThen, DiagnosticOnElse, DiagnosticOnException;

    public void Update(
        CursorText atPosition,
        string typename,
        object? primaryValue,
        IReadOnlyDictionary<string, object> constants,
        IDictionary<string, ServiceBuilder> branches)
    {
        if (this.Type != null || this.Position != null || this.Instance != null || this.Constants != null)
            throw new InvalidOperationException("cannot update dirty service");

        this.ServiceIdentity.Filename = atPosition.BareFile;
        this.ServiceIdentity.Position = atPosition.GetCurrentCol();
        this.ServiceIdentity.Line = atPosition.GetCurrentLine();
        this.ServiceIdentity.Typename = typename;

        this.Position = atPosition;
        this.TypeName = typename;
        this.CleanupPropagation = [.. branches.Values.Select<ServiceBuilder, Action>(x => x.Cleanup)];
        try
        {
            if (!atPosition.Policies.TryGetValue("shorthand", out var shorthandPolicyCandidate) ||
                shorthandPolicyCandidate is not ShorthandNamePolicy snp) snp = ShorthandNamePolicy.Rejected; 
            this.Instance = TypeRepository.Instance.CreateInstanceFor(this.TypeName, snp, out this.Type);
        }
        catch (MissingServiceTypeException ex)
        {
            throw new ExceptionAtPositionInFile(
                atPosition, $"""
                             In ({atPosition.BareFile}) {atPosition.WorkingDirectory} [{atPosition.GetCurrentLine()}:{atPosition.GetCurrentCol()}]
                             {ex.Message}
                             """);
        }

        this.Constants = new StampedMap(primaryValue ?? NullBuster, constants);

        this.Instance.OnThen += DiagnosticOnThen;
        CleanupPropagation.Add(() => this.Instance.OnThen -= DiagnosticOnThen);
        this.Instance.OnElse += DiagnosticOnElse;
        CleanupPropagation.Add(() => this.Instance.OnElse -= DiagnosticOnElse);
        this.Instance.OnException += DiagnosticOnException;
        CleanupPropagation.Add(() => this.Instance.OnException -= DiagnosticOnException);
        this.Instance.OnException += Instance_OnException;
        CleanupPropagation.Add(() => this.Instance.OnException -= Instance_OnException);

        var allEvents = Type.GetEvents().ToArray();
        this.EventHandlers = new SortedList<string, CallForInteraction>();
        foreach (var item in allEvents)
        {
            if (item.Name == "OnThen") this.OnThenEventInfo = item;
            if (item.Name == "OnElse") this.OnElseEventInfo = item;
            if (branches.TryGetValue(item.Name, out var child))
            {
                var newEvent = EventHandlers[item.Name] = child.Run;
                item.AddEventHandler(this.Instance, newEvent);
                CleanupPropagation.Add(() => item.RemoveEventHandler(this.Instance, newEvent));
            }
        }

        var strangeBranches = branches.Keys.ToList();
        var serviceBranches = allEvents.Select(x => x.Name).ToList();
        foreach (var item in serviceBranches)
            strangeBranches.Remove(item);
        if (strangeBranches.FirstOrDefault() is string strangeName)
            throw new ExceptionAtPositionInFile(atPosition,
                $"Branch {strangeName} does not exist on service {Type.Name}");
    }

    private void Instance_OnException(object sender, IInteraction interaction)
    {
        GlobalLog.Instance?.Error($"Service indicates exceptional situation; {JsonConvert.SerializeObject(
            new ExceptionPayload(Constants, Type, Position, interaction), Formatting.Indented)}");
    }

    public void OnThen(CallForInteraction dlg)
    {
        if (dlg.Target is IAmbiguousServiceWrapper asw)
        {
            CleanupPropagation!.Add(asw.Cleanup);
        }

        OnThenEventInfo!.AddEventHandler(Instance!, dlg);
        CleanupPropagation!.Add(() => OnThenEventInfo!.RemoveEventHandler(Instance!, dlg));
    }

    public void OnElse(CallForInteraction dlg)
    {
        if (dlg.Target is IAmbiguousServiceWrapper asw)
        {
            CleanupPropagation!.Add(asw.Cleanup);
        }

        OnElseEventInfo!.AddEventHandler(Instance!, dlg);
        CleanupPropagation!.Add(() => OnThenEventInfo!.RemoveEventHandler(Instance!, dlg));
    }

    public void OnDone(CallForInteraction dlg) => this.DoneDelegate =
        dlg == null ? dlg : throw new InvalidOperationException("Cant have two dones");

    bool isInCleanLoop = false;
    private readonly object cleanLock = new();

    public void Cleanup()
    {
        if (isInCleanLoop) return;
        if (CleanupPropagation != null)
        {
            lock (cleanLock)
            {
                isInCleanLoop = true;
                foreach (var item in CleanupPropagation)
                {
                    item();
                }

                CleanupPropagation.Clear();
                isInCleanLoop = false;
            }
        }

        if (Instance == null || EventHandlers == null || Type == null)
            throw new InvalidOperationException("Cant cleanup uninitialized service");
        this.DoneDelegate = null;
        var allEvents = Type.GetEvents().ToArray();
        foreach (var item in allEvents)
            if (EventHandlers.TryGetValue(item.Name, out var handler))
                item.RemoveEventHandler(this.Instance, handler);
        try
        {
            if (this.Instance is IDisposable disposable) disposable.Dispose();
        }
        catch (Exception e)
        {
            GlobalLog.Instance?.Error($"While disposing: {e}");
        }
    }

    public void Run(object sender, IInteraction interaction)
    {
        ServiceProfiler.Instance.Watch(ServiceIdentity, () =>
        {
            try
            {
                Instance!.Enter(Constants, interaction);
            }
#if !DEBUG || true
            catch (Exception ex)
            {
                GlobalLog.Instance?.Error(ex, "Fatal on {0}", Type?.Name ?? "Unknown Type");
                Instance!.HandleFatal(new CommonInteraction(interaction, ex.ToString()), ex);
            }
#endif
            finally
            {
                DoneDelegate?.DynamicInvoke(this, interaction);
            }
        });
    }

    public IEnumerable<(DefinedServiceWrapper wrapper, IService service)> GetAllServices()
    {
        if (Instance != null)
            return [(this, Instance)];
        else
            return [];
    }
}