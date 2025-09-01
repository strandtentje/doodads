namespace Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.StreamingUrlEncoded;

public class StreamByteEnumerator(Stream stream) : IEnumerator<byte>
{
    private int CurrentByteValue = -1;

    public byte Current => CurrentByteValue is > -1 and < 256
        ? (byte)CurrentByteValue
        : throw new InvalidOperationException("No byte value; MoveNext first or end of value");

    object IEnumerator.Current => Current;

    public bool MoveNext()
    {
        CurrentByteValue = stream.ReadByte();
        return CurrentByteValue != -1;
    }

    public void Reset() => throw new NotSupportedException();
    public void Dispose() { stream.Dispose(); }
}