using Microsoft.DevTunnels.Ssh;
using Buffer = Microsoft.DevTunnels.Ssh.Buffer;

namespace Ziewaar.RAD.Doodads.Cryptography.Ssh.Channels.Support;
public class SshChannelSendingStream(SshChannel channel) : Stream
{
    public override void Flush()
    {
    }
    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count)
    {
        if (channel.IsClosed) return;
        channel.SendAsync(Buffer.From(buffer, offset, count), CancellationToken.None).Wait();
    }

    public override bool CanRead => false;
    public override bool CanSeek => false;
    public override bool CanWrite => true;
    public override long Length => throw new NotImplementedException();
    public override long Position
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }
}