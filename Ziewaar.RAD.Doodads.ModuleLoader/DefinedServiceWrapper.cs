using Newtonsoft.Json;
using Ziewaar.RAD.Doodads.RKOP.Text;
namespace Ziewaar.RAD.Doodads.ModuleLoader;
#nullable enable
public class DefinedServiceWrapper : IAmbiguousServiceWrapper
{
    private Type? CurrentType;
    private SortedList<string, List<Delegate>> ExistingEventHandlers = new();
    private EventInfo? OnThenEventInfo, OnElseEventInfo;
    private Delegate? DoneDelegate;
    private CursorText? CurrentPosition;
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
        this.CurrentPosition = atPosition;
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

            if (ExistingEventHandlers.TryGetValue(item.Name, out var handlers))
                foreach (var handler in handlers)
                    item.RemoveEventHandler(this.Instance, handler);

            if (branches.TryGetValue(item.Name, out var child))
            {
                var newEvent = newEventHandlers[item.Name] = [child.Run];
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
        try
        {
            Instance!.Enter(Constants, interaction);
        } catch(Exception ex)
        {
            Console.WriteLine("Fatal on {0}", CurrentType?.Name ?? "Unknown Type");
            try
            {
                var errorPayload = new SortedList<string, object>
                {
                    { "type", CurrentType?.Name ?? "Unknown Type" },
                    { "directory", CurrentPosition?.WorkingDirectory.FullName ?? "?" },
                    { "file", CurrentPosition?.BareFile ?? "?" },
                    { "line", CurrentPosition?.GetCurrentLine() ?? -1 },
                    { "column", CurrentPosition?.GetCurrentCol() ?? -1 },
                };
                Console.WriteLine("Dump of Fatal {0}", JsonConvert.SerializeObject(errorPayload, Formatting.Indented));
                Instance!.HandleFatal(new CommonInteraction(interaction, ex.ToString(), errorPayload), ex);
            } catch(Exception metaEx)
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
            DoneDelegate!.DynamicInvoke(this, interaction);
        }
    }
}
	