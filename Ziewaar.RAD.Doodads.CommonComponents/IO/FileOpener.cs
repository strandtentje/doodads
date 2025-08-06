#nullable enable
#pragma warning disable 67
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Ziewaar.RAD.Doodads.CommonComponents.IO;

public static class FileOpener
{
    public static void OpenWithDefaultApp(string path)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Process.Start is enough on Windows, but requires some setup in .NET Core/5+
            var psi = new ProcessStartInfo(path)
            {
                UseShellExecute = true
            };
            Process.Start(psi);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Process.Start("open", $"\"{path}\"");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Process.Start("xdg-open", $"\"{path}\"");
        }
        else
        {
            throw new PlatformNotSupportedException("Unknown platform!");
        }
    }
}