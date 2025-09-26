using Microsoft.DevTunnels.Ssh.IO;
using Microsoft.DevTunnels.Ssh.Messages;

namespace Ziewaar.RAD.Doodads.Cryptography.Ssh.Channels.Support;
public class SshPfwOkMessage(uint actualPort) : SshMessage
{
    public override byte MessageType => 81;
    protected override void OnRead(ref SshDataReader reader)
    {
        throw new NotSupportedException();
    }
    protected override void OnWrite(ref SshDataWriter writer)
    {
        writer.Write(actualPort);
    }
}