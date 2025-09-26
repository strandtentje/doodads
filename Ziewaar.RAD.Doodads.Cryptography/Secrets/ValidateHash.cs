using Isopoh.Cryptography.Argon2;
using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;
using Ziewaar.RAD.Doodads.Cryptography.Secrets.Interactions;

namespace Ziewaar.RAD.Doodads.Cryptography.Secrets;
[Category("Tokens & Cryptography")]
[Title("Validate Password with Argon2")]
[Description("""
             When triggered with a sensitive interaction that's not been consumed (use LoadSensitive),
             And a password hash in the register,
             It will validate the sensitive string against the password hash.
             Argon2 is used.
             """)]
public class ValidateHash : IService
{
    [EventOccasion("When the hash and sensitive string matched")]
    public event CallForInteraction? OnThen;
    [EventOccasion("When the hash and sensitive string mismatched")]
    public event CallForInteraction? OnElse;
    [EventOccasion("When no sensitive string was found, or all of them were consumed")]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (!interaction.TryGetClosest<ISensitiveInteraction>(out ISensitiveInteraction? sensitive,
                x => x.TryVirginity()) ||
            sensitive?.GetSensitiveObject()?.ToString() is not string sensitiveValue)
        {
            OnException?.Invoke(this,
                new CommonInteraction(interaction, "can only validate sensitive strings; use LoadSensitive"));
            return;
        }

        if (Argon2.Verify(interaction.Register?.ToString(), sensitiveValue))
            OnThen?.Invoke(this, interaction);
        else
            OnElse?.Invoke(this, interaction);
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}