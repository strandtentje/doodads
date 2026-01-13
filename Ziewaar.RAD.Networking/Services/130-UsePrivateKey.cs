using Ziewaar.Network.Encryption;

namespace Ziewaar.RAD.Networking;

public class UsePrivateKey : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (!interaction.TryGetClosest(out CustomInteraction<PrivateKeyRepository>? keyStore) || keyStore == null)
            OnException?.Invoke(this, interaction.AppendRegister("Private key store must exist"));
        else if (interaction.Register is not string keyName || string.IsNullOrWhiteSpace(keyName))
            OnException?.Invoke(this, interaction.AppendRegister("Must have key name in register"));
        else
            OnThen?.Invoke(this,
                new CustomInteraction<RsaWrapper>(interaction, keyStore.Payload.LoadPrivateKey(keyName)));
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}