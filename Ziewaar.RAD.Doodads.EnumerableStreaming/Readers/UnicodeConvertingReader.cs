namespace Ziewaar.RAD.Doodads.EnumerableStreaming.Readers;
public class UnicodeConvertingReader(
    ICountingEnumerator<byte> binaryInput,
    long limit = -1) : ICountingEnumerator<char>
{
    public static UnicodeConvertingReader Empty = new(RootByteReader.Empty);
    public char Current { get; private set; }
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
        {
            this.AtEnd = binaryInput.AtEnd;
            this.ErrorState ??= binaryInput.ErrorState;
            return false;
        }
        if (!UpdateUnicodeChar())
            return false;
        Cursor++;
        return true;
    }
    private bool UpdateUnicodeChar()
    {
        var remainingReads = binaryInput.Current switch
        {
            < 0b1000_0000 => 0,
            >= 0b1100_0000 and < 0b1110_0000 => 1,
            >= 0b1110_0000 and < 0b1111_0000 => 2,
            >= 0b1111_0000 and < 0b1111_1000 => 3,
            _ => -1
        };

        if (remainingReads == -1)
        {
            ErrorState = "Byte was not the first in an UTF8-char";
            return false;
        }

        var charBytes = new List<byte>(remainingReads + 1) { binaryInput.Current };
        for (int i = 0; i < remainingReads; i++)
        {
            if (!binaryInput.MoveNext())
            {
                ErrorState = "Out of bytes while unicode character wasn't over";
                return false;
            }
            else if (binaryInput.Current is < 0b1000_0000 or >= 0b1100_0000)
            {
                ErrorState = $"Illegal {i}nth utf8 char";
                return false;
            }
            charBytes.Add(binaryInput.Current);
        }

        var chars = Encoding.UTF8.GetChars(charBytes.ToArray());
        if (chars.Length != 1)
        {
            ErrorState = $"Got more utf8 chars than expected from {remainingReads} bytes";
            return false;
        }
        Current = chars[0];
        if (Current == '\0')
        {
            ErrorState = "Null terminating";
            return false;
        }
        return true;
    }
    public void Reset() => throw new NotSupportedException("Cannot reset streaming text");
    object? IEnumerator.Current => Current;
    public void Dispose()
    {
        if (this.CanContinue())
            throw new InvalidOperationException("Leaving byte reader before it was fully read");
    }
}