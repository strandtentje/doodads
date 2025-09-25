namespace Ziewaar.RAD.Doodads.EnumerableStreaming.Readers;
public class ScopedByteReader(
    string description,
    ICountingEnumerator<byte> reader,
    long limit = -1,
    params byte[] terminators)
    : ICountingEnumerator<byte>
{
    public bool AtEnd { get; private set; }
    public bool MoveNext()
    {
        if (limit > -1 && Cursor >= limit)
            ErrorState = $"Read limit at {limit} bytes reached.";
        if (AtEnd || ErrorState != null) return false;
        if (!reader.MoveNext())
            return false;

        var lb = reader.Current;
        AtEnd = terminators.Contains(lb);
        if (AtEnd) return false;
        Current = (byte)lb;
        Cursor++;
        return true;
    }
    public void Reset() => throw new NotSupportedException("Cannot reset scoped byte reader");
    public byte Current { get; private set; }
    object? IEnumerator.Current => Current;
    public void Dispose()
    {
        if (!AtEnd && ErrorState == null)
            throw new InvalidOperationException("Leaving byte reader before it was fully read");
    }
    public long Cursor { get; private set; }
    public string? ErrorState { get => reader.ErrorState; set =>  reader.ErrorState = value; }
}