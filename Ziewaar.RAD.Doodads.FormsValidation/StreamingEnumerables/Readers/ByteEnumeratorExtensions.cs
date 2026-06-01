namespace Ziewaar.RAD.Doodads.EnumerableStreaming.Readers;
public static class ByteEnumeratorExtensions
{
    public static bool Expect(this ICountingEnumerator<byte> bytes, byte expected)
    {
        if (bytes.Current == expected) return true;
        bytes.ErrorState = $"Expected {BitConverter.ToString([expected])}";
        return false;
    }
    public static bool TryRenderToString(this ICountingEnumerator<char> chars, out string str)
    {
        var arr = chars.MakeEnumerable().ToArray();
        str = new string(arr);
        return chars.ErrorState == null;
    }
    private static IEnumerable<char> MakeEnumerable(this IEnumerator<char> chars)
    {
        while (chars.MoveNext())
            yield return chars.Current;
    }
    public static bool ExpectLowerAscii(this ICountingEnumerator<byte> bytes, out byte lowerByte)
    {
        lowerByte = bytes.Current;
        if (bytes.Current is < EXCL or > WGGL)
            bytes.ErrorState = "Out of lower ascii range";
        return bytes.ErrorState == null;
    }
    public static bool ExpectSaneAscii(this ICountingEnumerator<byte> bytes, out byte lowerByte)
    {
        lowerByte = bytes.Current;
        if (bytes.Current is < EXCL or 127)
            bytes.ErrorState = "Out of printable ascii range";
        return bytes.ErrorState == null;
    }
    public static bool ExpectUrlSafeAscii(this ICountingEnumerator<byte> bytes, out byte goodByteLiteral)
    {
        goodByteLiteral = bytes.Current;
        if (char.IsLetterOrDigit((char)goodByteLiteral) 
            || goodByteLiteral == '-' || goodByteLiteral == '_' || goodByteLiteral == '.' ||
            goodByteLiteral == '*')
            return true;
        bytes.ErrorState = "URL Unsafe Character detected";
        return false;
    }
    public static bool CanContinue<TAny>(this ICountingEnumerator<TAny> bytes) =>
        bytes is { AtEnd: false, ErrorState: null };
    private const byte
        HEX_0 = 0x30,
        HEX_9 = 0x39,
        HEX_UA = 0x41,
        HEX_UF = 0x46,
        HEX_LA = 0x61,
        HEX_LF = 0x66,
        EXCL = 0x21,
        WGGL = 0x7e;
    public static byte GetHexNibble(this ICountingEnumerator<byte> byteSource)
    {
        if (byteSource.Current >= HEX_0 && byteSource.Current <= HEX_9)
        {
            return (byte)(byteSource.Current - HEX_0);
        }
        else if (byteSource.Current >= HEX_UA && byteSource.Current <= HEX_UF)
        {
            return (byte)(byteSource.Current - HEX_UA + 10);
        }
        else if (byteSource.Current >= HEX_LA && byteSource.Current <= HEX_LF)
        {
            return (byte)(byteSource.Current - HEX_LA + 10);
        }
        else
        {
            byteSource.ErrorState = "Expected hex character";
            return 0;
        }
    }
    public static byte GetHexByte(this ICountingEnumerator<byte> byteSource)
    {
        byte msb = 0, lsb = 0;
        if (!byteSource.MoveNext())
            byteSource.ErrorState = "Interrupted in Percent Escape";
        else
            msb = byteSource.GetHexNibble();
        if (byteSource.ErrorState != null)
            return 0;

        if (!byteSource.MoveNext())
            byteSource.ErrorState = "Interrupted in Percent Escape";
        else
            lsb = byteSource.GetHexNibble();
        if (byteSource.ErrorState != null)
            return 0;
        return (byte)(msb * 16 + lsb);
    }
}