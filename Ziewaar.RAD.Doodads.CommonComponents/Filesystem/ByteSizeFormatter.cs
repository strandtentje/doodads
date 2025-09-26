#nullable enable
#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.CommonComponents.IO;

public static class ByteSizeFormatter
{
    private static readonly string[] SizeSuffixes =
        { "bytes", "KB", "MB", "GB", "TB", "PB", "EB" };

    public static string ToHumanReadable(long byteCount)
    {
        if (byteCount < 0)
            throw new ArgumentException("Byte count cannot be negative.");

        if (byteCount == 0)
            return "0 bytes";

        int i = 0;
        decimal value = byteCount;

        while (value >= 1024 && i < SizeSuffixes.Length - 1)
        {
            value /= 1024;
            i++;
        }

        return $"{value:n2} {SizeSuffixes[i]}";
    }
}
