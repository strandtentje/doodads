using Isopoh.Cryptography.Argon2;
using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;

namespace Ziewaar.RAD.Doodads.Cryptography;
[Category("Cryptography")]
[Title("Hash Password with Argon2")]
[Description("""
             When triggered with a sensitive interaction that's not been consumed (use LoadSensitive), 
             It will hash the string in the sensitive interaction for use as a password hash.
             Argon2 is used.
             """)]
public class PasswordHash : IService
{
    [EventOccasion("When the password was hashed")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [EventOccasion("When there was no sensitive string to hash")]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (!interaction.TryGetClosest<ISensitiveInteraction>(out ISensitiveInteraction? sensitive, x => x.TryVirginity()) ||
            sensitive?.GetSensitiveObject()?.ToString() is not string sensitiveValue)
        {
            OnException?.Invoke(this,
                new CommonInteraction(interaction, "can only hash sensitive strings; use LoadSensitive"));
            return;
        }
        var hashed = Argon2.Hash(sensitiveValue);
        OnThen?.Invoke(this, new CommonInteraction(interaction, hashed));
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}