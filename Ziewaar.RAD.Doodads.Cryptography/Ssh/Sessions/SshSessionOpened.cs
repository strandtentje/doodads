using System.Security.Claims;
using Microsoft.DevTunnels.Ssh.Events;
using Ziewaar.RAD.Doodads.Cryptography.Ssh.Server.Support;
using Ziewaar.RAD.Doodads.Cryptography.Ssh.Sessions.Support;

namespace Ziewaar.RAD.Doodads.Cryptography.Ssh.Sessions;

public class SshSessionOpened : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (!interaction.TryGetClosest<SshServerInteraction>(out var serverInteraction) || serverInteraction == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "SSH server interaction required"));
            return;
        }

        serverInteraction.SshServer.SessionOpened += (_, session) =>
        {
            session.Authenticating += (_, args) =>
            {
                if (args.AuthenticationType != SshAuthenticationType.ClientPublicKey &&
                    args.AuthenticationType != SshAuthenticationType.ClientPublicKeyQuery)
                    args.AuthenticationTask =
                        Task.FromException<ClaimsPrincipal?>(
                            new UnauthorizedAccessException("Only publickey allowed"));
            };
            OnThen?.Invoke(this, new SshSessionInteraction(interaction, session));
        };
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}