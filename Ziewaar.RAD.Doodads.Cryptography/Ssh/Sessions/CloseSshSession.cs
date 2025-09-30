namespace Ziewaar.RAD.Doodads.Cryptography.Ssh.Sessions;

[Category("Networking & Connections")]
[Title("Close SSH Session")]
[Description("""
             Provided an SSH Session, closes it if it isn't already 
             and continue at OnThen
             """)]
public class CloseSshSession : IService
{
    [EventOccasion("When the connection was closed correctly")]
    public event CallForInteraction? OnThen;
    [EventOccasion("When the connection was already closed")]
    public event CallForInteraction? OnElse;
    [EventOccasion("When there was no connection to close, " +
                   "or closing didn't go so well.")]
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

        if (!sessionInteraction.Session.IsClosed)
        {
            try
            {
                sessionInteraction.Session
                    .CloseAsync(SshDisconnectReason.ByApplication).Wait();
                OnThen?.Invoke(this, interaction);
            }
            catch (Exception ex)
            {
                OnException?.Invoke(this,
                    new CommonInteraction(interaction, ex));
            }
        }
        else
        {
            OnElse?.Invoke(this, interaction);
        }
    }

    public void HandleFatal(IInteraction source, Exception ex) =>
        OnException?.Invoke(this, source);
}