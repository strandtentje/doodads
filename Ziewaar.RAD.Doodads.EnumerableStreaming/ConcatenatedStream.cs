using Ziewaar.RAD.Doodads.CoreLibrary;

namespace Ziewaar.RAD.Doodads.EnumerableStreaming;
#pragma warning disable 67
public sealed class ConcatenatedStream(params Stream[] streams) : Stream, IFinishSensingStream
{
    private int CurrentStreamNumber;
    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => throw new NotSupportedException();

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public override int ReadTimeout
    {
        get => streams.Min(x => x.ReadTimeout);
        set
        {
            foreach (Stream stream in streams)
                stream.ReadTimeout = value;
        }
    }

    public override void Flush() => throw new NotSupportedException();

    public override int Read(byte[] buffer, int offset, int count)
    {
        while (CurrentStreamNumber < streams.Length)
        {
            if (streams[CurrentStreamNumber] is not IFinishSensingStream finishSenser ||
                !finishSenser.IsFinished)
            {
                int r = streams[CurrentStreamNumber].Read(buffer, offset, count);
                if (r > 0) return r;
            }
            CurrentStreamNumber++;
        }

        return 0;
    }

    public override long Seek(long offset, SeekOrigin origin) =>
        throw new NotSupportedException();

    public override void SetLength(long value) =>
        throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) =>
        throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (var s in streams)
                s.Dispose();
        }

        base.Dispose(disposing);
    }

    public bool IsFinished =>
        CurrentStreamNumber >= streams.Length - 1
        && streams[CurrentStreamNumber] is IFinishSensingStream { IsFinished: true };
}