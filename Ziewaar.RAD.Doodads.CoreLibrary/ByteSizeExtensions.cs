#nullable enable
using System.Globalization;
using System.Text.RegularExpressions;

namespace Ziewaar.RAD.Doodads.CoreLibrary;

public static class ByteSizeExtensions
{
    private static readonly string[] SizeSuffixes = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };

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

public static class ByteSizeParsingExtensions
{
    private static ulong GetMultiplierByExponentSuffix(string suffix) => suffix.ToUpper().TrimEnd('B').Replace(" ", "") switch
    {
        "" => 1,

        "K" => (ulong)Math.Pow(1000, 1),
        "M" => (ulong)Math.Pow(1000, 2),
        "G" => (ulong)Math.Pow(1000, 3),
        "T" => (ulong)Math.Pow(1000, 4),
        "P" => (ulong)Math.Pow(1000, 5),
        "E" => (ulong)Math.Pow(1000, 6),
        "Z" => (ulong)Math.Pow(1000, 7),
        "Y" => (ulong)Math.Pow(1000, 8),

        "KI" => (ulong)Math.Pow(1024, 1),
        "MI" => (ulong)Math.Pow(1024, 2),
        "GI" => (ulong)Math.Pow(1024, 3),
        "TI" => (ulong)Math.Pow(1024, 4),
        "PI" => (ulong)Math.Pow(1024, 5),
        "EI" => (ulong)Math.Pow(1024, 6),
        "ZI" => (ulong)Math.Pow(1024, 7),
        "YI" => (ulong)Math.Pow(1024, 8),

        _ => throw new FormatException("Invalid byte size marker"),
    };
    public static long ParseByteSize(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Input cannot be null or empty.", nameof(input));

        StringBuilder whole = new(), sym = new(), deci = new();

        for (int index = 0; index < input.Length; index++)
        {
            char current = input[index];
            if (char.IsDigit(current))
            {
                if (sym.Length == 0)
                    whole.Append(current);
                else 
                    deci.Append(current);
            } else if (current == '.')
            {
                sym.Append(' ');
            } else if (char.IsLetter(current))
            {
                sym.Append(current);
            }
        }

        var multiplier = GetMultiplierByExponentSuffix(sym.ToString());
        if (!decimal.TryParse($"{whole}.{deci}", NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat, out var number))
            throw new FormatException("Expected number");

        return (long)Math.Floor(number * multiplier);
    }
}