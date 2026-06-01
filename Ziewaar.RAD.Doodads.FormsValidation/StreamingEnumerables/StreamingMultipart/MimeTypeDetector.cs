using Ziewaar.RAD.Doodads.EnumerableStreaming.Readers;

namespace Ziewaar.RAD.Doodads.EnumerableStreaming.StreamingMultipart;

public class MimeTypeDetector : ICountingEnumerator<byte>
{
    private readonly ICountingEnumerator<byte> ByteSource;
    private readonly Queue<byte> PrefetchedBytes = new(512);
    public readonly string DetectedMime;
    public readonly bool IsText;

    public MimeTypeDetector(ICountingEnumerator<byte> byteSource)
    {
        ByteSource = byteSource;
        while(PrefetchedBytes.Count < 512 && byteSource.MoveNext())
            PrefetchedBytes.Enqueue(ByteSource.Current);
        DetectedMime = SignatureMimeGuesser.GuessMimeType(PrefetchedBytes.ToList(), out var isText);
        IsText = isText;
    }
    public byte Current => _current;
    object IEnumerator.Current => _current;
    public bool AtEnd { get; private set; } = false;
    private long _cursor; private byte _current;
    private bool _isErrorState;
    private bool _outOfPrefetchBytes;

    public long Cursor => _cursor;
    public string? ErrorState
    {
        get
        {
            return ByteSource.ErrorState;
        }

        set
        {
            this._isErrorState = value != null;
            ByteSource.ErrorState = value;
        }
    }
    public bool MoveNext()
    {
        if (_isErrorState) return false;

        if (!_outOfPrefetchBytes)
        {
            _current = PrefetchedBytes.Dequeue();
            _outOfPrefetchBytes = PrefetchedBytes.Count == 0;
            _cursor++;
            return true;
        }

        if (ByteSource.MoveNext())
        {
            _current = ByteSource.Current;
            _cursor++;
            return true;
        }

        AtEnd = ByteSource.AtEnd;
        return false;
    }

    public void Reset() => throw new InvalidOperationException("Cant reset");
    public void Dispose()
    {


    }
}