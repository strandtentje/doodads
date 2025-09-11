namespace Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.Readers;
public class UrlEncodedValueBytesReader(
    ICountingEnumerator<byte> binaryInput,
    long limit = -1) : ICountingEnumerator<byte>
{
    private const byte EQUALS = 0x3d,
        AMPERSAND = 0x26,
        PLUS = 0x2B,
        SPACE = 0x20,
        PERC = 0x25;
    public byte Current { get; private set; }
    public bool AtEnd { get; private set; }
    public long Cursor { get; private set; }
    public string? ErrorState { get => binaryInput.ErrorState; set =>  binaryInput.ErrorState = value; }
    public bool MoveNext()
    {
        if (limit > -1 && Cursor >= limit)
            ErrorState = $"Read limit at {limit} bytes reached.";
        
        if (AtEnd || ErrorState != null || 
            binaryInput.AtEnd || binaryInput.ErrorState != null ||
            !binaryInput.MoveNext())
            return false;
        
        switch (binaryInput.Current)
        {
            case AMPERSAND:
            case EQUALS:
                AtEnd = true;
                break;
            case PLUS:
                Current = SPACE;
                break;
            case PERC:
                Current = binaryInput.GetHexByte();
                break;
            default:
                if (binaryInput.ExpectUrlSafeAscii(out var c))
                    Current = c;
                break;
        }
        this.ErrorState = binaryInput.ErrorState;
        Cursor++;
        return this.CanContinue();
    }
    public void Reset() => throw new NotSupportedException("Cannot reset scoped reader");
    public void Dispose()
    {
        if (this.CanContinue())
            throw new InvalidOperationException("Leaving byte reader before it was fully read");
    }
    object? IEnumerator.Current => Current;
}