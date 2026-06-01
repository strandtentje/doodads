namespace Ziewaar.RAD.Doodads.EnumerableStreaming.Readers;

public class RootByteReader(Stream stream, long limit = -1)
    : ICountingEnumerator<byte>, ILongPosition
{
    public bool AtEnd => _atEnd;
    private bool _atEnd;
    public long Limit => limit;
    public static ICountingEnumerator<byte> Empty = new RootByteReader(new MemoryStream([]));
    private readonly byte[] PreBuffer = new byte[4096*16];
    private int PreBufferCursor = 0;
    private int PreBufferEndstop = 0;

    public bool MoveNext()
    {
        if (limit > -1 && _cursor >= limit)
            _errorState = $"Read limit at {limit} bytes reached.";
        if (_atEnd || _errorState != null) return false;

        if (PreBufferCursor >= PreBufferEndstop)
        {
            PreBufferEndstop = stream.Read(PreBuffer, 0, PreBuffer.Length);
            PreBufferCursor = 0;
        }

        if (PreBufferCursor >= PreBufferEndstop)
        {
            _atEnd = true;
            return false;
        }

        _current = PreBuffer[PreBufferCursor++];
        _cursor++;
        return true;
    }

    public void Reset()
    {
        ErrorState = null;
        _cursor = 0;
        _atEnd = false;
    }

    private byte _current;
    public byte Current => _current;

    object? IEnumerator.Current => Current;

    public void Dispose()
    {
        if (!AtEnd && ErrorState == null)
            throw new InvalidOperationException("Leaving byte reader before it was fully read");
    }
    private long _cursor;
    public long Cursor => _cursor;
    private string? _errorState;
    public string? ErrorState { get => _errorState; set => _errorState = value; }
}