namespace Ziewaar.RAD.Doodads.ModuleLoader;
#nullable enable
public class DefinedServiceWrapper : IAmbiguousServiceWrapper
{
    private Type? CurrentType;
    private SortedList<string, List<Delegate>> ExistingEventHandlers = new();
    private EventInfo? OnThenEventInfo, OnElseEventInfo, OnExceptionEventInfo;
    private Delegate? DoneDelegate;

    public string? TypeName { get; private set; }
    public IService? Instance { get; private set; }
    public StampedMap? Constants { get; private set; }
    public event EventHandler<IInteraction>? DiagnosticOnThen, DiagnosticOnElse, DiagnosticOnException;
    public void Update(
        CursorText atPosition,
        string typename,
        object? primaryValue,
        SortedList<string, object> constants,
        IDictionary<string, ServiceBuilder> branches)
    {
        if (this.TypeName != typename || this.Instance == null || this.CurrentType == null || this.TypeName == null)
        {
            Cleanup();
            this.TypeName = typename;
            this.Instance = TypeRepository.Instance.CreateInstanceFor(this.TypeName, out this.CurrentType);
        }
        this.Constants = new StampedMap(constants);

        this.Instance.OnThen += DiagnosticOnThen;
        this.Instance.OnElse += DiagnosticOnElse;
        this.Instance.OnException += DiagnosticOnException;

        var allEvents = CurrentType.GetEvents().ToArray();
        var newEventHandlers = new SortedList<string, List<Delegate>>();
        foreach (var item in allEvents)
        {
            if (item.Name == "OnThen") this.OnThenEventInfo = item;
            if (item.Name == "OnElse") this.OnElseEventInfo = item;
            if (item.Name == "OnException") this.OnExceptionEventInfo = item;

            if (ExistingEventHandlers.TryGetValue(item.Name, out var handlers))
                foreach (var handler in handlers)
                    item.RemoveEventHandler(this.Instance, handler);

            if (branches.TryGetValue(item.Name, out var child))
            {
                var newEvent = newEventHandlers[item.Name] = [new EventHandler<IInteraction>(child.Run)];
                item.AddEventHandler(this.Instance, newEvent[0]);
            }
        }
        ExistingEventHandlers = newEventHandlers;
    }
    public void OnThen(Delegate dlg) => OnThenEventInfo!.AddEventHandler(Instance!, dlg);
    public void OnElse(Delegate dlg) => OnElseEventInfo!.AddEventHandler(Instance!, dlg);
    public void OnDone(Delegate dlg) => this.DoneDelegate =
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
    public void Run(IInteraction interaction)
    {
        Instance!.Enter(Constants, interaction);
        DoneDelegate!.DynamicInvoke(this, interaction);
    }
}