namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services;

public class UrlAccessGuarantor
{
    public static void EnsureUrlAcls(IEnumerable<string> prefixes)
    {
        foreach (var prefix in prefixes)
        {
            if (!IsPrefixReserved(prefix))
            {
                Console.WriteLine($"Prefix not registered: {prefix}");
                AddUrlAcl(prefix);
            }
            else if (!IsNotOnWindows)
            {                
                Console.WriteLine($"Prefix already registered: {prefix}");
            }
        }
    }

    private static bool IsPrefixReserved(string prefix)
    {
        if (IsNotOnWindows)
            return true;
        var output = RunProcessAndCapture("netsh", "http show urlacl");
        var normalizedPrefix = prefix.TrimEnd('/');

        var ourPrefixPosition = output.IndexOf(normalizedPrefix);
        if (ourPrefixPosition >= 0)
        {
            var positionOfNewLine = output.IndexOf("\n", ourPrefixPosition);
            var positionOfUser = output.IndexOf("User", positionOfNewLine);
            var positionOfNextNewline = output.IndexOf("\n", positionOfUser);
            var bitThatShouldContainOurUsername = output.Substring(positionOfUser, positionOfNextNewline - positionOfUser);

#pragma warning disable CA1416 // Validate platform compatibility; IsNotOnWindows catches this.
            return bitThatShouldContainOurUsername.Contains(WindowsIdentity.GetCurrent().Name);
#pragma warning restore CA1416 // Validate platform compatibility
        }
        else
        {
            return false;
        }
    }

    private static bool IsNotOnWindows => Path.DirectorySeparatorChar != '\\' || !OperatingSystem.IsWindows();

    private static void AddUrlAcl(string prefix)
    {
        if (IsNotOnWindows)
            return;
#pragma warning disable CA1416 // Validate platform compatibility; IsNotOnWindows catches this.
        var username = WindowsIdentity.GetCurrent().Name;
#pragma warning restore CA1416 // Validate platform compatibility
        var arguments = $"http add urlacl url={prefix} user=\"{username}\"";

        var psi = new ProcessStartInfo
        {
            FileName = "netsh",
            Arguments = arguments,
            Verb = "runas", // triggers UAC just for netsh
            UseShellExecute = true
        };

        try
        {
            Process.Start(psi)?.WaitForExit();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to add URL ACL for {prefix}: {ex.Message}");
        }
    }

    private static string RunProcessAndCapture(string fileName, string arguments)
    {
        var psi = new ProcessStartInfo(fileName, arguments)
        {
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (var p = Process.Start(psi))
        {
            return p!.StandardOutput.ReadToEnd();
        }
    }
}
