#nullable enable
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Ziewaar.RAD.Starter;

public static class ExecutableResolver
{
    public static string FindAssociatedExecutable(string dllPath)
    {
        if (string.IsNullOrEmpty(dllPath) || !File.Exists(dllPath))
            throw new FileNotFoundException($"Assembly location '{dllPath}' does not exist.");

        var dir = Path.GetDirectoryName(dllPath)
            ?? throw new InvalidOperationException($"Cannot determine directory for assembly '{dllPath}'.");

        var baseName = Path.GetFileNameWithoutExtension(dllPath);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var candidateExe = Path.Combine(dir, baseName + ".exe");
            if (File.Exists(candidateExe))
                return candidateExe;

            throw new FileNotFoundException($"Expected executable '{candidateExe}' not found.");
        }
        else
        {
            var files = Directory.GetFiles(dir);

            // Exact name match with +x
            var exactMatch = files.FirstOrDefault(f =>
                Path.GetFileName(f) == baseName &&
                IsExecutableUnix(f));

            if (exactMatch != null)
                return exactMatch;

            // Fallback: any +x file with matching base name
            var fallback = files.FirstOrDefault(f =>
                Path.GetFileNameWithoutExtension(f) == baseName &&
                IsExecutableUnix(f));

            if (fallback != null)
                return fallback;

            throw new FileNotFoundException($"No executable matching '{baseName}' found in '{dir}'.");
        }
    }

    private static bool IsExecutableUnix(string path)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "/bin/sh",
            Arguments = $"-c \"[ -x \\\"{path}\\\" ]\"",
            RedirectStandardOutput = false,
            RedirectStandardError = false,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        try
        {
            using var proc = Process.Start(psi);
            proc.WaitForExit();
            return proc.ExitCode == 0;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to check executability of '{path}'", ex);
        }
    }
}
