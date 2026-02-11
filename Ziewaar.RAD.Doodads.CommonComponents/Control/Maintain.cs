#nullable enable
using System.Threading.Tasks;

namespace Ziewaar.RAD.Doodads.CommonComponents.Control;

[Category("Scheduling & Flow")]
[Title("Tend to a long running task with an interval")]
[Description("""
    Works like recurring but doesnt block
    """)]
public class Maintain : IService, IDisposable
{
    [NamedSetting("ms", "ms to delay with")]
    private readonly UpdatingKeyValue DelayInMsConstant = new("ms");
    private decimal CurrentDelay;    
    private readonly Recurring Recurring = new();
    public Maintain()
    {
        Recurring.OnThen += InvokeOnThen;
        Recurring.OnElse += InvokeOnElse;
        Recurring.OnException += InvokeOnException;
    }

    private void InvokeOnException(object sender, IInteraction interaction) => OnException?.Invoke(sender, interaction);
    private void InvokeOnElse(object sender, IInteraction interaction) => OnElse?.Invoke(sender, interaction);
    private void InvokeOnThen(object sender, IInteraction interaction) => OnThen?.Invoke(sender, interaction);

    [EventOccasion("Tick")]
    public event CallForInteraction? OnThen;
    [EventOccasion("Sink interval string here")]
    public event CallForInteraction? OnElse;
    [NeverHappens]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        Task.Run(() => Recurring.Enter(constants, interaction));
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
    public void Dispose()
    {
        Recurring.Dispose();
    }
}
