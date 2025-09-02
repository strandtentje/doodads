namespace Ziewaar.RAD.Doodads.CoreLibrary;

public static class ByteSizeExtensions
{
    private static readonly string[] SizeSuffixes =
        { "B", "KB", "MB", "GB", "TB", "PB", "EB" };

    public static string ToByteSizeString(this long byteCount, int decimals = 1)
    {
        if (byteCount < 0)
            return "-" + ((ulong)(-byteCount)).ToByteSizeString(decimals);

        return ((ulong)byteCount).ToByteSizeString(decimals);
    }

    public static string ToByteSizeString(this ulong byteCount, int decimals = 1)
    {
        if (byteCount == 0)
            return "0 B";

        int mag = (int)Math.Log(byteCount, 1024);
        decimal adjustedSize = (decimal)byteCount / (1UL << (mag * 10));

        return $"{Math.Round(adjustedSize, decimals)} {SizeSuffixes[mag]}";
    }

    // Signed overloads
    public static string ToByteSizeString(this int byteCount, int decimals = 1) =>
        ((long)byteCount).ToByteSizeString(decimals);

    public static string ToByteSizeString(this short byteCount, int decimals = 1) =>
        ((long)byteCount).ToByteSizeString(decimals);

    public static string ToByteSizeString(this sbyte byteCount, int decimals = 1) =>
        ((long)byteCount).ToByteSizeString(decimals);

    public static string ToByteSizeString(this double byteCount, int decimals = 1) =>
        ((long)byteCount).ToByteSizeString(decimals);

    public static string ToByteSizeString(this float byteCount, int decimals = 1) =>
        ((long)byteCount).ToByteSizeString(decimals);

    public static string ToByteSizeString(this decimal byteCount, int decimals = 1) =>
        ((long)byteCount).ToByteSizeString(decimals);

    // Unsigned overloads
    public static string ToByteSizeString(this uint byteCount, int decimals = 1) =>
        ((ulong)byteCount).ToByteSizeString(decimals);

    public static string ToByteSizeString(this ushort byteCount, int decimals = 1) =>
        ((ulong)byteCount).ToByteSizeString(decimals);

    public static string ToByteSizeString(this byte byteCount, int decimals = 1) =>
        ((ulong)byteCount).ToByteSizeString(decimals);
}