namespace Ziewaar.RAD.Doodads.Cryptography.Ssh.Sessions;
#pragma warning disable 67

[Category("Networking & Connections")]
[Title("Detect SSH Session Closed")]
[Description("""
             Triggers when an SSH session is closed.
             If the scoped SSH session already is closed, it'll trigger
             immediately.
             """)]
public class SshSessionClosed : IService
{
    [EventOccasion("When the SSH session closed")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [EventOccasion("When there wasn't an SSH session to detect closure on.")]
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (!interaction.TryGetClosest<SshSessionInteraction>(
                out var sessionInteraction) || sessionInteraction == null)
        {
            OnException?.Invoke(this,
                new CommonInteraction(interaction,
                    "SSH session interaction required"));
            return;
        }

        if (sessionInteraction.Session.IsClosed ||
            !sessionInteraction.Session.IsConnected)
        {
            OnThen?.Invoke(this, interaction);
        }
        else
        {
            sessionInteraction.Session.Closed += (_, _) =>
            {
                OnThen?.Invoke(this, interaction);
            };
        }
    }

    public void HandleFatal(IInteraction source, Exception ex) =>
        OnException?.Invoke(this, source);
}