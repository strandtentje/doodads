using System.IO.Pipelines;
using Microsoft.DevTunnels.Ssh;
using Microsoft.DevTunnels.Ssh.Events;
using Buffer = Microsoft.DevTunnels.Ssh.Buffer;

namespace Ziewaar.RAD.Doodads.Cryptography;
public class SshChannelReceivingStream : Stream, IDisposable
{
    private Pipe ChannelPipe;
    private readonly SshChannel SshChannel;
    private readonly IWindowingChannel WindowingChannel;
    private readonly Stream ChannelPipeReadStream;
    private ulong CurrentWindowSize;
    private ulong CurrentReceiveCount;
    private readonly uint WindowIncrement;
    private const uint DEFAULT_PIPE_UPPER = 64 * 1024;
    private const uint DEFAULT_PIPE_LOWER = 8 * 1024;
    private const uint DEFAULT_WINDOW_INCREMENT = 8 * 1024;
    public SshChannelReceivingStream(
        SshChannel channelToRemoteServer,
        uint currentWindowSize,
        uint pipeUpperLimit = DEFAULT_PIPE_UPPER,
        uint pipeLowerLimit = DEFAULT_PIPE_LOWER,
        uint windowIncrement = DEFAULT_WINDOW_INCREMENT)
    {
        this.CurrentWindowSize = currentWindowSize;
        this.WindowIncrement = windowIncrement;
        this.SshChannel = channelToRemoteServer;
        this.WindowingChannel = channelToRemoteServer;
        this.ChannelPipe = new(new PipeOptions(pauseWriterThreshold: pipeUpperLimit, resumeWriterThreshold: pipeLowerLimit));
        this.ChannelPipeReadStream = this.ChannelPipe.Reader.AsStream();
        this.SshChannel.DataReceived += SshChannelOnDataReceived;
        this.SshChannel.Closed += SshChannelOnClosed;
    }
    private void SshChannelOnClosed(object? sender, SshChannelClosedEventArgs e)
    {
        this.SshChannel.Closed -= SshChannelOnClosed;
        this.SshChannel.DataReceived -= SshChannelOnDataReceived;
        ChannelPipe.Writer.Complete();
    }
    private void SshChannelOnDataReceived(object? sender, Buffer e)
    {
        if (e.Count == 0)
        {
            try
            {
                SshChannel.SendAsync(Buffer.Empty, CancellationToken.None).Wait();
                SshChannel.CloseAsync().Wait();
            }
            finally
            {
                this.SshChannel.DataReceived -= SshChannelOnDataReceived;
                ChannelPipe.Writer.Complete();
            }
            return;
        }
        else
        {
            CurrentReceiveCount += (ulong)e.Count;
        }
        var writeResult = ChannelPipe.Writer.WriteAsync(
            new ReadOnlyMemory<byte>(e.Array, e.Offset, e.Count)).Result;

        if (CurrentReceiveCount + this.WindowIncrement > CurrentWindowSize)
        {
            WindowingChannel.IncreaseWindowSize(this.WindowIncrement);
            CurrentWindowSize += this.WindowIncrement;
        }

        if (writeResult.IsCanceled || writeResult.IsCompleted)
            SshChannel.SendAsync(Buffer.Empty, CancellationToken.None).Wait();
    }
    public override void Flush() => throw new NotSupportedException();
    public override int Read(byte[] buffer, int offset, int count) =>
        ChannelPipeReadStream.Read(buffer, offset, count);
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length { get => throw new NotSupportedException(); }
    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        ChannelPipeReadStream.Dispose();
    }
}