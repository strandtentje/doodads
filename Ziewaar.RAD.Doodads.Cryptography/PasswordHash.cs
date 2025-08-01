using Isopoh.Cryptography.Argon2;
namespace Ziewaar.RAD.Doodads.Cryptography;
public class PasswordHash : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
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