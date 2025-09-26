using System.Net;
using Microsoft.DevTunnels.Ssh;
using Ziewaar.RAD.Doodads.CoreLibrary;

namespace Ziewaar.RAD.Doodads.Cryptography;
public class SshSessionInteraction(IInteraction interaction, SshServerSession session) : IInteraction
{
    public IInteraction Stack => interaction;
    public object Register => interaction.Register;
    public IReadOnlyDictionary<string, object> Memory { get; } = new SwitchingDictionary(
        ["remotehost", "remoteversion", "remoteprotocol"], key =>
            key switch
            {
                "remotehost" => (session.RemoteEndpoint as IPEndPoint)?.Address?.ToString() ?? "",
                "remoteversion" => session.RemoteVersion?.Name ?? "",
                "remoteprotocol" => session.RemoteVersion?.ProtocolVersion.ToString() ?? "",
                _ => throw new KeyNotFoundException(),
            });
    public SshServerSession Session => session;
}