#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.Control;
public abstract class TimerCommandSender(TimerCommand command) : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        OnThen?.Invoke(this, new TimerCommandInteraction(interaction, command));
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
