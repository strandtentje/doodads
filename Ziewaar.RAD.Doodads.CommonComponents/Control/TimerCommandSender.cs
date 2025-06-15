using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;

#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.Control;

public abstract class TimerCommandSender(TimerCommand command) : IService
{
    [EventOccasion("Immediately continues here. The first timer encountered will be controlled by this command.")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [NeverHappens]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        OnThen?.Invoke(this, new TimerCommandInteraction(interaction, command));
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
