using Ziewaar.RAD.Doodads.Cryptography.Ssh.Server.Support;
#pragma warning disable 67

namespace Ziewaar.RAD.Doodads.Cryptography.Ssh.Server;
public class SshServerStart : IService
{
    private readonly UpdatingPrimaryValue PortNumberConstant = new();
    private ushort CurrentPortNumber;
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, PortNumberConstant).IsRereadRequired(out decimal portNumberCandidate))
            CurrentPortNumber = (ushort)Math.Clamp(portNumberCandidate, ushort.MinValue, ushort.MaxValue);
        if (!interaction.TryGetClosest<SshServerInteraction>(out var serverInteraction) || serverInteraction == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "ssh server setup required"));
            return;
        }
        _ = serverInteraction.SshServer.AcceptSessionsAsync(CurrentPortNumber);
        OnThen?.Invoke(this, new CommonInteraction(interaction, register: CurrentPortNumber));
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}