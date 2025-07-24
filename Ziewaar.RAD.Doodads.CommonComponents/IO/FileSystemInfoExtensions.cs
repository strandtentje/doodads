#nullable enable
#pragma warning disable 67
using System.Runtime.InteropServices;

namespace Ziewaar.RAD.Doodads.CommonComponents.IO;

public static class FileSystemInfoExtensions
{
    /// <summary>
    /// Determines if the file or directory is hidden, cross-platform.
    /// </summary>
    public static bool IsHidden(this FileSystemInfo info)
    {
        if (info == null) throw new ArgumentNullException(nameof(info));

        // On Windows, use the Hidden attribute
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return (info.Attributes & FileAttributes.Hidden) != 0;
        }
        else
        {
            // On Unix, hidden files start with '.'
            return info.Name.StartsWith(".", StringComparison.Ordinal);
        }
    }
}