#nullable enable
using System.Threading.Tasks;

namespace Ziewaar.RAD.Doodads.CommonComponents.Control;

[Category("Scheduling & Flow")]
[Title("Prevent blocking subsequent tasks")]
[Description("""
             Use this to prevent services under OnThen from blocking, instead delegating the 
             finished mark for OnThen. Sort of the opposite of Hold/Release
             """)]
public class Fork : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
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
