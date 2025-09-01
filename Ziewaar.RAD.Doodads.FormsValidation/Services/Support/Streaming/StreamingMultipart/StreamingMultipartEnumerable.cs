namespace Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.StreamingUrlEncoded;

public class StreamingMultipartEnumerable(Stream stream, byte[] boundary)
    : IEnumerable<StreamingMultipartDataValueGroup>
{
    private readonly object EnumeratorLock = new();
    private IEnumerator<StreamingMultipartDataValueGroup>? CurrentEnumerator = null;

    public IEnumerator<StreamingMultipartDataValueGroup> GetEnumerator()
    {
        lock (EnumeratorLock)
        {
            if (CurrentEnumerator != null)
                throw new InvalidOperationException("Streaming multipart data may only be enumerated once.");
            CurrentEnumerator = new StreamingMultipartEnumerator(stream, boundary);
        }

        return CurrentEnumerator;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}