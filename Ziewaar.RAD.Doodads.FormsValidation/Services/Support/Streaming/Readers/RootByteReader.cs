namespace Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.Readers;

public class RootByteReader(Stream stream, long limit = -1, params byte[] terminators)
    : ICountingEnumerator<byte>, ILongPosition
{
    public bool AtEnd { get; private set; }
    public long Limit => limit;
    public static ICountingEnumerator<byte> Empty = new RootByteReader(new MemoryStream([]));
    private readonly byte[] PreBuffer = new byte[4096];
    private int PreBufferCursor = 0;
    private int PreBufferEndstop = 0;

    public bool MoveNext()
    {
        if (limit > -1 && Cursor >= limit)
            ErrorState = $"Read limit at {limit} bytes reached.";
        if (AtEnd || ErrorState != null) return false;

        if (PreBufferCursor >= PreBufferEndstop)
        {
            PreBufferEndstop = stream.Read(PreBuffer, 0, PreBuffer.Length);
            PreBufferCursor = 0;
        }

        if (PreBufferCursor >= PreBufferEndstop || terminators.Contains(Current))
        {
            AtEnd = true;
            return false;
        }

        Current = PreBuffer[PreBufferCursor++];
        Cursor++;
        return true;
    }

    public void Reset()
    {
        ErrorState = null;
        Cursor = 0;
        AtEnd = false;
    }

    public byte Current { get; private set; }
    object? IEnumerator.Current => Current;

    public void Dispose()
    {
        if (!AtEnd && ErrorState == null)
            throw new InvalidOperationException("Leaving byte reader before it was fully read");
    }

    public long Cursor { get; private set; }
    public string? ErrorState { get; set; }
}