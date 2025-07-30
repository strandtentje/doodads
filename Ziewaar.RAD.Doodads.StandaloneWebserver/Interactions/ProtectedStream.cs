namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Interactions;

public class ProtectedStream(Stream original, HttpListenerContext context) : Stream
{
    public override bool CanRead => AssertConnectionAlive(() => original.CanRead);
    public override bool CanWrite => AssertConnectionAlive(() => original.CanWrite);
    public override bool CanSeek => false;
    public override long Length => AssertConnectionAlive(() => original.Length);

    public override int Read(byte[] buffer, int offset, int count) =>
        AssertConnectionAlive(() => original.Read(buffer, offset, count));

    public override void Write(byte[] buffer, int offset, int count)
    {
        AssertConnectionAlive(() => true);
        original.Write(buffer, offset, count);
    }

    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
        await AssertConnectionAlive(() => original.WriteAsync(buffer, offset, count, cancellationToken));

    private TResult AssertConnectionAlive<TResult>(Func<TResult> func)
    {
        try
        {
            if (!context.Response.OutputStream.CanWrite || !context.Request.InputStream.CanRead)
                throw new ConnectionDeadException();
        }
        catch (Exception ex)
        {
            throw new ConnectionDeadException(ex);
        }

        return func();
    }

    public override void Flush()
    {
        AssertConnectionAlive(() => true);
        original.Flush();
    }

    public override Task FlushAsync(CancellationToken cancellationToken) =>
        AssertConnectionAlive(() => original.FlushAsync(cancellationToken));

    public override long Position
    {
        get => AssertConnectionAlive(() => original.Position);
        set
        {
            if (!original.CanSeek)
                throw new NotSupportedException();
            AssertConnectionAlive(() => true);
            original.Position = value;
        }
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        if (!original.CanSeek)
            throw new NotSupportedException();
        AssertConnectionAlive(() => true);
        return original.Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
        AssertConnectionAlive(() => true);
        original.SetLength(value);
    }
}