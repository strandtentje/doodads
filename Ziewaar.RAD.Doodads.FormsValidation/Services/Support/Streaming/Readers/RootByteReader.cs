namespace Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.Readers;
public class RootByteReader(Stream stream, int limit = -1, params byte[] terminators) : ICountingEnumerator<byte>
{
    public bool AtEnd { get; private set; }
    private readonly long StartingPosition = stream.Position;
    public static ICountingEnumerator<byte> Empty = new RootByteReader(new MemoryStream([]));
    public bool MoveNext()
    {
        if (limit > -1 && Cursor >= limit)
            ErrorState = $"Read limit at {limit} bytes reached.";
        if (AtEnd || ErrorState != null) return false;
        var lb = stream.ReadByte();
        AtEnd = lb < 0 || lb == 0 || terminators.Contains((byte)lb);
        if (AtEnd) return false;
        Current = (byte)lb;
        Cursor++;
        return true;
    }
    public void Reset()
    {
        ErrorState = null;
        Cursor = 0;
        AtEnd = false;
        stream.Position = StartingPosition;
    }
    public byte Current { get; private set; }
    object? IEnumerator.Current => Current;
    public void Dispose()
    {
        if (!AtEnd && ErrorState == null)
            throw new InvalidOperationException("Leaving byte reader before it was fully read");
    }
    public int Cursor { get; private set; }
    public string? ErrorState { get; set; }
}