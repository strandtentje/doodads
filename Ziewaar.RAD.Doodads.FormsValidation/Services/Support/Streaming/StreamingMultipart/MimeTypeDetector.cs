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
    public byte Current { get; set; }
    object IEnumerator.Current => Current;
    public bool AtEnd { get; private set; } = false;
    public long Cursor { get; private set; } = 0;
    public string? ErrorState { get => ByteSource.ErrorState; set =>  ByteSource.ErrorState = value; }
    public bool MoveNext()
    {
        if (ErrorState != null) return false;

        if (PrefetchedBytes.Count > 0)
        {
            Current = PrefetchedBytes.Dequeue();
            Cursor++;
            return true;
        }

        if (ByteSource.MoveNext())
        {
            Current = ByteSource.Current;
            Cursor++;
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