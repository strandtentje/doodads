using Isopoh.Cryptography.Argon2;

namespace Ziewaar.RAD.Doodads.Cryptography;
public class ValidateHash : IService
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