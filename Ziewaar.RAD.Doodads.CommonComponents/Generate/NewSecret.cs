#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.Generate;

[Category("Tokens & Cryptography")]
[Title("New Secret")]
[Description("""Generate a Secret and stick it into the register.""")]
public class NewSecret : IService
{
    [EventOccasion("When the secret is available in register")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [NeverHappens]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        OnThen?.Invoke(this, new CommonInteraction(interaction, SecureRandomHex.Generate()));
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}

