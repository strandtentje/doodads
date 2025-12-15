#nullable enable
using System.Threading.Tasks;

namespace Ziewaar.RAD.Doodads.CommonComponents.Control;

[Category("Scheduling & Flow")]
[Title("Prevent blocking subsequent tasks")]
[Description("""
             Use this to prevent services under OnThen from blocking, instead delegating the 
             finished mark for OnThen. Sort of the opposite of Hold/Release. Is not very realtime
             or deterministic and may hold a bit before working.
             """)]
public class Fork : IService
{
    [EventOccasion("When the dotnet runtime task scheduler figures it's time to go.")]
    public event CallForInteraction? OnThen;
    [EventOccasion("When the primary task's ready.")]
    public event CallForInteraction? OnElse;
    [NeverHappens]
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        Task.Run(() =>
        {
            OnThen?.Invoke(this, interaction);
            OnElse?.Invoke(this, interaction);
        });
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
