using Ziewaar.RAD.Doodads.CommonComponents.LiteralSource;
using Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.Readers;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic;

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
    public long Cursor => _cursor;
    public string? ErrorState { get => ByteSource.ErrorState; set =>  ByteSource.ErrorState = value; }
    public bool MoveNext()
    {
        if (ErrorState != null) return false;

        if (PrefetchedBytes.Count > 0)
        {
            _current = PrefetchedBytes.Dequeue();
            _cursor++;
            return true;
        }

        if (ByteSource.MoveNext())
        {
            _cursor = ByteSource.Current;
            _current++;
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