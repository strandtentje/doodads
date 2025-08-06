#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.TextActions;

[Category("Tokens & Cryptography")]
[Title("New GUID")]
[Description("""Generate a GUID and stick it into the register.""")]
public class NewGuid : IService
{
    [EventOccasion("When the guid is available in register")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [NeverHappens]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction) => OnThen?.Invoke(this, new CommonInteraction(interaction, Guid.NewGuid().ToString()));
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}

