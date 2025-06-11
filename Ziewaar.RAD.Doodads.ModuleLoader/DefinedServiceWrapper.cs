#nullable enable
using Newtonsoft.Json;
using Ziewaar.RAD.Doodads.RKOP.Exceptions;
using Ziewaar.RAD.Doodads.RKOP.Text;
namespace Ziewaar.RAD.Doodads.ModuleLoader;

public class DefinedServiceWrapper : IAmbiguousServiceWrapper
{
    private static readonly object NullBuster = new();
    private Type? Type;
    private EventInfo? OnThenEventInfo, OnElseEventInfo;
    private CallForInteraction? DoneDelegate;
    private CursorText? Position;
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
        SortedList<string, object> constants,
        IDictionary<string, ServiceBuilder> branches)
    {
        if (this.Type != null || this.Position != null || this.Instance != null || this.Constants != null)
            throw new InvalidOperationException("cannot update dirty service");
        this.Position = atPosition;
        this.TypeName = typename;
        this.CleanupPropagation = [.. branches.Values.Select<ServiceBuilder, Action>(x => x.Cleanup)];
        try
        {
            this.Instance = TypeRepository.Instance.CreateInstanceFor(this.TypeName, out this.Type);
        } catch(MissingServiceTypeException ex)
        {
            throw new ExceptionAtPositionInFile(atPosition, $"""
                In ({atPosition.BareFile}) {atPosition.WorkingDirectory} [{atPosition.GetCurrentLine()}:{atPosition.GetCurrentCol()}]
                {ex.Message}
                """);
        }

        this.Constants = new StampedMap(primaryValue ?? NullBuster, constants);

        this.Instance.OnThen += DiagnosticOnThen;
        this.Instance.OnElse += DiagnosticOnElse;
        this.Instance.OnException += DiagnosticOnException;
        this.Instance.OnException += Instance_OnException;

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
            }
        }
        var strangeBranches = branches.Keys.ToList();
        var serviceBranches = allEvents.Select(x => x.Name).ToList();
        foreach (var item in serviceBranches)
            strangeBranches.Remove(item);
        if (strangeBranches.FirstOrDefault() is string strangeName)
            throw new ExceptionAtPositionInFile(atPosition, $"Branch {strangeName} does not exist on service {Type.Name}");
    }
    private void Instance_OnException(object sender, IInteraction interaction)
    {
        Console.WriteLine(
            "Service indicates exceptional situation; {0}",
            JsonConvert.SerializeObject(
                new ExceptionPayload(
                    Constants,
                    Type, Position, interaction),
                Formatting.Indented));
    }
    public void OnThen(CallForInteraction dlg)
    {
        if (dlg.Target is IAmbiguousServiceWrapper asw)
        {
            CleanupPropagation!.Add(asw.Cleanup);
        }
        OnThenEventInfo!.AddEventHandler(Instance!, dlg);
    }
    public void OnElse(CallForInteraction dlg)
    {
        if (dlg.Target is IAmbiguousServiceWrapper asw)
        {
            CleanupPropagation!.Add(asw.Cleanup);
        }
        OnElseEventInfo!.AddEventHandler(Instance!, dlg);
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
            Console.Write($"While disposing: {e}");
        }
    }
    public void Run(object sender, IInteraction interaction)
    {
        try
        {
            Instance!.Enter(Constants, interaction);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Fatal on {0}", Type?.Name ?? "Unknown Type");
            try
            {
                Console.WriteLine("Dump of Fatal {0}", JsonConvert.SerializeObject(ex, Formatting.Indented));
                Instance!.HandleFatal(new CommonInteraction(interaction, ex.ToString()), ex);
            }
            catch (Exception metaEx)
            {
                Console.WriteLine("Exception while handling exception {0}; {1}", ex, metaEx);
#if DEBUG
                throw;
#endif
            }
#if DEBUG
            throw;
#endif
        }
        finally
        {
            DoneDelegate?.DynamicInvoke(this, interaction);
        }
    }
}
