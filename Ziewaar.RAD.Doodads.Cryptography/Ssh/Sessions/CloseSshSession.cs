using Microsoft.DevTunnels.Ssh;

namespace Ziewaar.RAD.Doodads.Cryptography;

public class CloseSshSession : IService
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

        if (!sessionInteraction.Session.IsClosed)
        {
            try
            {
                sessionInteraction.Session.CloseAsync(SshDisconnectReason.ByApplication).Wait();
                OnThen?.Invoke(this, interaction);
            }
            catch (Exception ex)
            {
                OnException?.Invoke(this, new CommonInteraction(interaction, ex));
            }
        }
        else
        {
            OnElse?.Invoke(this, interaction);
        }
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}