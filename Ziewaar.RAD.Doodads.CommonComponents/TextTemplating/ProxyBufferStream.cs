#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.TextTemplating;
public class ProxyBufferStream : Stream
{
    private readonly Stream _underlying;
    private readonly MemoryStream _buffer = new MemoryStream();
    private bool _finalFlushed = false;

    public ProxyBufferStream(Stream underlying)
    {
        _underlying = underlying ?? throw new ArgumentNullException(nameof(underlying));
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        if (_finalFlushed)
            throw new InvalidOperationException("Cannot write after FinalFlush");
        _buffer.Write(buffer, offset, count);
    }

    public void FinalFlush()
    {
        if (_finalFlushed) return;
        _buffer.Position = 0;
        _buffer.CopyTo(_underlying);
        _underlying.Flush();
        _finalFlushed = true;
        _buffer.SetLength(0); // Optionally clear buffer
    }

    // For completeness: implement other required abstract members
    public override bool CanRead => false;
    public override bool CanSeek => false;
    public override bool CanWrite => !_finalFlushed;
    public override long Length => _buffer.Length;
    public override long Position { get => _buffer.Position; set => throw new NotSupportedException(); }
    public override void Flush() { /* Ignore, do nothing */ }
    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _buffer.Dispose();
            // do not dispose underlying stream here unless you own it
        }
        base.Dispose(disposing);
    }
}