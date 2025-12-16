#nullable enable
#pragma warning disable 67
using System.Threading;
using System.Threading.Tasks;
using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;

namespace Ziewaar.RAD.Doodads.CommonComponents.Control;

[Category("Scheduling & Flow")]
[Title("Interval repetition")]
[Description("Like maintain, but will not wait for the work to complete. So more like Maintain+Fork")]
public class Interval : IService
{
    [NamedSetting("ms", "ms to delay with")]
    private readonly UpdatingKeyValue DelayInMsConstant = new("ms");
    public Interval()
    {
        Maintain.OnThen += (s, e) =>
        {
            Task.Run(() =>
            {
                OnThen?.Invoke(this, e);
            });
        };
    }
    
    private readonly Maintain Maintain = new();
    [EventOccasion("The job to run")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [NeverHappens]
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        Maintain.Enter(constants, interaction);
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
