using Microsoft.DevTunnels.Ssh;
using Microsoft.DevTunnels.Ssh.IO;
using Microsoft.DevTunnels.Ssh.Messages;

namespace Ziewaar.RAD.Doodads.Cryptography.Ssh.Channels.Support;
public class SshPfwReqMessage(
    string serverGotConnectionOnAddress,
    uint serverGotConnectionOnPort,
    string serverGotConnectionFromAddress,
    uint serverGotConnectionFromPort)
    : ChannelOpenMessage
{
    public override byte MessageType => 90;
    protected override void OnRead(ref SshDataReader reader)
    {
        throw new NotSupportedException();
    }
    protected override void OnWrite(ref SshDataWriter writer)
    {
        writer.Write("forwarded-tcpip", Encoding.ASCII);
        writer.Write(SenderChannel);
        writer.Write(SshChannel.DefaultMaxWindowSize);
        writer.Write(SshChannel.DefaultMaxPacketSize);
        writer.Write(serverGotConnectionOnAddress, Encoding.ASCII);
        writer.Write(serverGotConnectionOnPort);
        writer.Write(serverGotConnectionFromAddress, Encoding.ASCII);
        writer.Write(serverGotConnectionFromPort);
    }
}