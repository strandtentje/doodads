using Ziewaar.RAD.Doodads.CommonComponents.LiteralSource;
using Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.Readers;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic;

public class MimeTypeDetector : ICountingEnumerator<byte>
{
    private readonly ICountingEnumerator<byte> ByteSource;
    private readonly List<byte> PrefetchedBytes = new(512);
    public readonly string DetectedMime;
    public readonly bool IsText;

    public MimeTypeDetector(ICountingEnumerator<byte> byteSource)
    {
        ByteSource = byteSource;
        while(PrefetchedBytes.Count < 512 & byteSource.MoveNext())
            PrefetchedBytes.Add(ByteSource.Current);
        this.DetectedMime = SignatureMimeGuesser.GuessMimeType(PrefetchedBytes, out var isText);
        this.IsText = isText;
    }
    public byte Current { get; set; }
    object? IEnumerator.Current => Current;
    public bool AtEnd { get; private set; } = true;
    public long Cursor { get; private set; } = 0;
    public string? ErrorState { get => ByteSource.ErrorState; set =>  ByteSource.ErrorState = value; }
    public bool MoveNext()
    {
        if (ErrorState != null) return false;
        
        if (Cursor < PrefetchedBytes.Count)
        {
            Current = PrefetchedBytes[(int)Cursor++];
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
        if (this.CanContinue())
            GlobalLog.Instance?.Warning("Attempting to dispose of counting enumerator that isn't ceased yet");
    }
}