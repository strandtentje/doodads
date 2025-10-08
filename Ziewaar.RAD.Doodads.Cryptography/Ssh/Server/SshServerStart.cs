#pragma warning disable 67

namespace Ziewaar.RAD.Doodads.Cryptography.Ssh.Server;

[Category("Networking & Connections")]
[Title("Start SSH server")]
[Description("""
             Provided a ssh server that's all setup and never started,
             start it. This is typically done after anything else you'd
             want to setup with an SSH server.
             """)]
public class SshServerStart : IService
{
    [PrimarySetting("Port number to listen on")]
    private readonly UpdatingPrimaryValue PortNumberConstant = new();
    private ushort CurrentPortNumber;
    [NeverHappens]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [EventOccasion("When no SSH server was setup, or listening for connections broke donwn.")]
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
        using (serverInteraction.SshServer)
        {
            OnThen?.Invoke(this, new CommonInteraction(interaction, register: CurrentPortNumber));
            try
            {
                serverInteraction.SshServer.AcceptSessionsAsync(CurrentPortNumber).Wait();
            }
            catch (Exception ex)
            {
                OnException?.Invoke(this, new CommonInteraction(interaction, ex));
            }
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}