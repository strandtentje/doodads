using Ziewaar.RAD.Doodads.Cryptography.Ssh.Sessions.Support;

namespace Ziewaar.RAD.Doodads.Cryptography.Ssh.Sessions;
#pragma warning disable 67

public class SshSessionClosed : IService
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

        sessionInteraction.Session.Closed += (_, _) =>
        {
            OnThen?.Invoke(this, interaction);
        };
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}