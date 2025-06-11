#nullable enable
using Newtonsoft.Json;
using Ziewaar.RAD.Doodads.RKOP.Text;
namespace Ziewaar.RAD.Doodads.ModuleLoader;

public class DefinedServiceWrapper : IAmbiguousServiceWrapper
{
    private static readonly object NullBuster = new();
    private Type? CurrentType;
    private SortedList<string, List<CallForInteraction>> ExistingEventHandlers = new();
    private EventInfo? OnThenEventInfo, OnElseEventInfo;
    private CallForInteraction? DoneDelegate;
    private CursorText? CurrentPosition;
    private CallForInteraction? PreviousOnThen, PreviousOnElse;
    public string? TypeName { get; private set; }
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
        this.CurrentPosition = atPosition;
        if (this.TypeName != typename || this.Instance == null || this.CurrentType == null || this.TypeName == null)
        {
            Cleanup();
            this.TypeName = typename;
            this.Instance = TypeRepository.Instance.CreateInstanceFor(this.TypeName, out this.CurrentType);
        }
        this.Constants = new StampedMap(primaryValue ?? NullBuster, constants);

        this.Instance.OnThen += DiagnosticOnThen;
        this.Instance.OnElse += DiagnosticOnElse;
        this.Instance.OnException += DiagnosticOnException;
        this.Instance.OnException += Instance_OnException;

        var allEvents = CurrentType.GetEvents().ToArray();
        var newEventHandlers = new SortedList<string, List<CallForInteraction>>();
        foreach (var item in allEvents)
        {
            if (item.Name == "OnThen") this.OnThenEventInfo = item;
            if (item.Name == "OnElse") this.OnElseEventInfo = item;

            if (ExistingEventHandlers.TryGetValue(item.Name, out var handlers))
                foreach (var handler in handlers)
                    item.RemoveEventHandler(this.Instance, handler);
            item.

            if (branches.TryGetValue(item.Name, out var child))
            {
                var newEvent = newEventHandlers[item.Name] = [child.Run];
                item.AddEventHandler(this.Instance, newEvent[0]);
            }
        }
        ExistingEventHandlers = newEventHandlers;
    }
    private void Instance_OnException(object sender, IInteraction interaction)
    {
        Console.WriteLine(
            "Service indicates exceptional situation; {0}", 
            JsonConvert.SerializeObject(
                new ExceptionPayload(
                    Constants,
                    CurrentType, CurrentPosition, interaction), 
                Formatting.Indented));
    }
    public void OnThen(CallForInteraction dlg)
    {
        if (this.PreviousOnThen != null)
            OnThenEventInfo!.RemoveEventHandler(Instance!, this.PreviousOnThen);
        
        OnThenEventInfo!.AddEventHandler(Instance!, dlg);
        this.PreviousOnThen = dlg;
    }
    public void OnElse(CallForInteraction dlg)
    {
        if (this.PreviousOnElse != null)
            OnThenEventInfo!.RemoveEventHandler(Instance!, this.PreviousOnElse);
        
        OnElseEventInfo!.AddEventHandler(Instance!, dlg);
        this.PreviousOnElse = dlg;
    }
    public void OnDone(CallForInteraction dlg) => this.DoneDelegate =
        dlg == null ? dlg : throw new InvalidOperationException("Cant have two dones");
    public void Cleanup()
    {
        this.DoneDelegate = null;
        if (CurrentType != null && this.Instance != null)
        {
            var allEvents = CurrentType.GetEvents().ToArray();
            foreach (var item in allEvents)
                if (ExistingEventHandlers.TryGetValue(item.Name, out var handlers))
                    foreach (var handler in handlers)
                        item.RemoveEventHandler(this.Instance, handler);
        }
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
            Console.WriteLine("Fatal on {0}", CurrentType?.Name ?? "Unknown Type");
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
