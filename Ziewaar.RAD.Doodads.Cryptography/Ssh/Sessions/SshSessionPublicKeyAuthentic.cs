using Microsoft.DevTunnels.Ssh.Events;
using Microsoft.DevTunnels.Ssh.Keys;

namespace Ziewaar.RAD.Doodads.Cryptography;
public class SshSessionPublicKeyAuthentic : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (!interaction.TryGetClosest<SshSessionInteraction>(out var sessionInteraction) || sessionInteraction == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "SSH session interaction required"));
            return;
        }

        var formatter = new Pkcs8KeyFormatter();

        sessionInteraction.Session.Authenticating += (sender, args) =>
        {
            if (args.AuthenticationType != SshAuthenticationType.ClientPublicKey || args.PublicKey == null)
                return;
        };
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}