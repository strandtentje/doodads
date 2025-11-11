using System.IO.Pipelines;
using System.Buffers;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Buffer = Microsoft.DevTunnels.Ssh.Buffer;

namespace Ziewaar.RAD.Doodads.Cryptography.Ssh.Channels.Support;

public class HolderOf<TStruct>(TStruct str) where TStruct : struct
{
    public TStruct Struct = str;
}

public class SshChannelReceivingStream : Stream, IDisposable, IFinishSensingStream
{
    private readonly SshChannel SshChannel;
    private readonly IWindowingChannel WindowingChannel;
    private readonly Queue<HolderOf<Buffer>> ReceivedBuffers = new();
    private static readonly HolderOf<Buffer> Empty = new HolderOf<Buffer>([]);
    private readonly Lock QueueLock = new();

    private readonly EventWaitHandle BlockUntilItems =
        new EventWaitHandle(initialState: false, EventResetMode.ManualReset);

    private bool IsDisposed;
    private int LastReadSize = 1024;
    private readonly EventWaitHandle TriggerToIncreaseWindow = new(false, EventResetMode.AutoReset);

    public SshChannelReceivingStream(SshChannel channelToRemoteServer)
    {
        this.SshChannel = channelToRemoteServer;
        this.WindowingChannel = channelToRemoteServer;
        this.SshChannel.DataReceived += SshChannelOnDataReceived;
        this.SshChannel.Closed += SshChannelOnClosed;
        
        Task.Run(() =>
        {
            try
            {
                while (!IsFinished)
                {
                    while (!TriggerToIncreaseWindow.WaitOne(Moment))
                        if (IsFinished)
                            return;
                    WindowingChannel.IncreaseWindowSize((uint)LastReadSize);
                }
            }
            catch (Exception)
            {
                // whatever
            }
        });
    }

    private void SshChannelOnClosed(object? sender, SshChannelClosedEventArgs e)
    {
        lock (this.QueueLock)
        {
            if (!IsDisposed)
            {
                ReceivedBuffers.Enqueue(Empty);
                BlockUntilItems.Set();
            }
        }

        this.SshChannel.DataReceived -= SshChannelOnDataReceived;
        this.SshChannel.Closed -= SshChannelOnClosed;

    }

    private void SshChannelOnDataReceived(object? sender, Buffer e)
    {
        lock (QueueLock)
        {
            if (IsDisposed)
            {
                SshChannel.SendAsync(Buffer.Empty, CancellationToken.None).ConfigureAwait(false);
            }
            else
            {
                ReceivedBuffers.Enqueue(new(e.Copy()));
                BlockUntilItems.Set();
            }
        }
    }

    public override void Flush() => throw new NotSupportedException();

    private static readonly TimeSpan Moment = TimeSpan.FromMilliseconds(100);
    public bool IsFinished => IsDisposed || SshChannel.IsClosed;

    public override int Read(byte[] buffer, int offset, int count)
    {
        LastReadSize = count;
        TriggerToIncreaseWindow.Set();

        HolderOf<Buffer>? wrapper;
        while (!ReceivedBuffers.TryPeek(out wrapper))
        {
            if (!BlockUntilItems.WaitOne(Moment))
            {
                if (IsDisposed || SshChannel.IsClosed)
                    throw new ObjectDisposedException(nameof(SshChannelReceivingStream));
            }
        }

        lock (QueueLock)
        {
            int byteCountReceived = wrapper.Struct.Count;
            if (byteCountReceived == 0)
            {
                ReceivedBuffers.Dequeue();
                if (!IsFinished)
                    BlockUntilItems.Reset();
                return 0;
            }

            if (count < byteCountReceived)
            {
                Array.Copy(wrapper.Struct.Array, wrapper.Struct.Offset, buffer, offset, count);
                wrapper.Struct.Offset += count;
                wrapper.Struct.Count -= count;
                return count;
            }
            else if (count == byteCountReceived)
            {
                Array.Copy(wrapper.Struct.Array, wrapper.Struct.Offset, buffer, offset, count);
                ReceivedBuffers.Dequeue();
                if (!IsFinished)
                    BlockUntilItems.Reset();
                return count;
            }
            else if (count > byteCountReceived)
            {
                Array.Copy(wrapper.Struct.Array, wrapper.Struct.Offset, buffer, offset, byteCountReceived);
                ReceivedBuffers.Dequeue();
                if (!IsFinished)
                    BlockUntilItems.Reset();
                return byteCountReceived;
            }
            else
            {
                throw new InvalidOperationException(
                    "Read count wasn't lower, bigger, or equal. This shouldn't happen.");
            }
        }
    }

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length { get => throw new NotSupportedException(); }

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    protected override void Dispose(bool disposing)
    {
        lock (QueueLock)
        {
            this.IsDisposed = true;
            BlockUntilItems.Set();
            BlockUntilItems.Dispose();
            TriggerToIncreaseWindow.Dispose();
            base.Dispose(disposing);
        }
    }
}