namespace Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.StreamingUrlEncoded;

public class StreamingMultipartEnumerator(Stream stream, byte[] boundary)
    : IEnumerator<StreamingMultipartDataValueGroup>
{
    private readonly PartReader Reader = new(stream, boundary);
    private StreamingMultipartDataValueGroup? CurrentGroup = null;

    public StreamingMultipartDataValueGroup Current =>
        CurrentGroup ?? throw new InvalidOperationException("No group; MoveNext first");

    object? IEnumerator.Current => Current;

    public bool MoveNext()
    {
        var part = Reader.ReadNextPart();
        if (part == null)
            return false;
        CurrentGroup = new StreamingMultipartDataValueGroup(part.Header, part.Body);
        return true;
    }

    public void Reset() => throw new NotSupportedException();
    public void Dispose() { }
}