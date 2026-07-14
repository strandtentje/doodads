#pragma warning disable 67
using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices;

namespace Ziewaar.RAD.Doodads.CommonComponents.Filesystem.Iterating;

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
    public static string GetNumberPrefix(this FileSystemInfo fsi) => new([.. fsi.Name.TakeWhile(char.IsDigit)]);
    public static string GetAfterNumberPrefix(this FileSystemInfo fsi) =>
        new([.. fsi.Name.SkipWhile(char.IsDigit).SkipWhile(c => c == '_' || c == '-')]);

    public static bool TryGetNumberPrefix(this FileSystemInfo fsi, out string pfx, out string rem)
    {
        pfx = GetNumberPrefix(fsi);
        rem = GetAfterNumberPrefix(fsi);
        return pfx.Length > 0;
    }

    public static IEnumerable<FileSystemInfo> GetSiblingsOfSameType(this FileSystemInfo fsi) => fsi switch
    {
        DirectoryInfo di => di.Parent?.GetDirectories().OfType<FileSystemInfo>() ?? [],
        FileInfo fi => fi.Directory?.GetFiles().OfType<FileSystemInfo>() ?? [],
        _ => throw new ArgumentException("expected fileinfo or directoryinfo", nameof(fsi))
    };

    public static void GetNextSplittable(this FileSystemInfo fsi, out string pfx, out string rem)
    {
        var selfAndSiblingsAfter = fsi.GetSiblingsOfSameType().
            OrderBy(x => x.Name).SkipWhile(x => x.Name != fsi.Name);
        var siblingsAfter = selfAndSiblingsAfter.Skip(1);
        foreach (var item in siblingsAfter)
            if (item.TryGetNumberPrefix(out pfx, out rem))
                return;
        pfx = "";
        rem = "";
        return;
    }
    public static void GetPrevSplittable(this FileSystemInfo fsi, out string pfx, out string rem)
    {
        var selfAndSiblingsAfter = fsi.GetSiblingsOfSameType().
            OrderByDescending(x => x.Name).SkipWhile(x => x.Name != fsi.Name);
        var siblingsAfter = selfAndSiblingsAfter.Skip(1);
        foreach (var item in siblingsAfter)
            if (item.TryGetNumberPrefix(out pfx, out rem))
                return;
        pfx = "";
        rem = "";
        return;
    }
}