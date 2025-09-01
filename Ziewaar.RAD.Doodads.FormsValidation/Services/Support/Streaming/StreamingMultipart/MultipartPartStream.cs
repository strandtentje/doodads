namespace Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.StreamingUrlEncoded;

public class MultipartPartStream(Stream source, byte[] boundary) : Stream
{
    private readonly StreamBoundarySlicer Scanner = new(source, boundary);
    private bool EndReached = false;
    public override int Read(byte[] buffer, int offset, int count)
    {
        if (EndReached) return 0;
        int read = Scanner.ReadUntilDelimiter(buffer, offset, count);
        if (read == 0) EndReached = true;
        return read;
    }
    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => throw new NotSupportedException();

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public override void Flush() => throw new NotSupportedException();
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
}