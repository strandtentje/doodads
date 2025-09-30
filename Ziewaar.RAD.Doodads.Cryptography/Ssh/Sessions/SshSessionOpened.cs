namespace Ziewaar.RAD.Doodads.Cryptography.Ssh.Sessions;

[Category("Networking & Connections")]
[Title("Detect SSH Session Opened")]
[Description("""
             Triggers when an SSH session is opened on a server.
             It'll demand publickey authentication by default, so badly 
             opened sessions that aren't otherwise handled, will die.
             """)]
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