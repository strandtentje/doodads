namespace Ziewaar.RAD.Doodads.AdvancedFilesystem;
using System.IO;
using System.Runtime.InteropServices;

public static class SymlinkHelper
{
    public static bool IsSymlink(string path)
    {
        var attr = File.GetAttributes(path);
        return attr.HasFlag(FileAttributes.ReparsePoint);
    }

    public static string? ResolveSymlinkTarget(string path)
    {
        if (!IsSymlink(path))
            return null;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Best-effort: Windows symlinks are reparse points
            var info = new FileInfo(path);
            return info.LinkTarget ?? info.FullName;
        }
        else
        {
            // Use 'readlink' on Unix
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "readlink",
                Arguments = "-f \"" + path + "\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var proc = System.Diagnostics.Process.Start(psi);
            proc.WaitForExit();
            var result = proc.StandardOutput.ReadToEnd().Trim();

            return string.IsNullOrWhiteSpace(result) ? null : result;
        }
    }
}
