namespace Ziewaar.RAD.Doodads.Cryptography.Ssh.Sessions.Support;
public class SshSessionInteraction(IInteraction interaction, SshServerSession session) : IInteraction
{
    public IInteraction Stack => interaction;
    public object Register => interaction.Register;
    public IReadOnlyDictionary<string, object> Memory { get; } = new SwitchingDictionary(
        ["remotehost", "remoteversion", "remoteprotocol", "sshsessionstate"], key =>
            key switch
            {
                "remotehost" => (session.RemoteEndpoint as IPEndPoint)?.Address?.ToString() ?? "",
                "remoteversion" => session.RemoteVersion?.Name ?? "",
                "remoteprotocol" => session.RemoteVersion?.ProtocolVersion.ToString() ?? "",
                "sshsessionstate" => session.IsConnected && !session.IsClosed ? "connected" : "dead",
                _ => throw new KeyNotFoundException(),
            });
    public SshServerSession Session => session;
}