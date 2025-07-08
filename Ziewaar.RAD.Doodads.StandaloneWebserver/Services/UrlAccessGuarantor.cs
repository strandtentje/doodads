namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services;

public class UrlAccessGuarantor
{
    public static void EnsureUrlAcls(IEnumerable<string> prefixes)
    {
        foreach (var prefix in prefixes)
        {
            switch (IsPrefixReserved(prefix))
            {
                case ReservationState.UrlAvailableForThisUser:
                    Console.WriteLine($"Prefix already registered: {prefix}");
                    break;
                case ReservationState.ReservationNotNeccesary:
                    Console.WriteLine($"Not on windows, no need to reserve urls");
                    break;
                case ReservationState.UrlReservedForSomeoneElse:
                    Console.WriteLine($"URL was reserved for someone else {prefix}. Claiming it.");
                    RemoveUrlAcl(prefix);
                    AddUrlAcl(prefix);
                    break;
                case ReservationState.UrlNotReserved:
                    Console.WriteLine($"URL was not reserved {prefix}. Claiming it.");
                    AddUrlAcl(prefix);
                    break;

            }
        }
    }

    public enum ReservationState { ReservationNotNeccesary, UrlAvailableForThisUser, UrlReservedForSomeoneElse, UrlNotReserved };

    private static ReservationState IsPrefixReserved(string prefix)
    {
        if (IsNotOnWindows)
            return ReservationState.ReservationNotNeccesary;
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
            if (bitThatShouldContainOurUsername.Contains(WindowsIdentity.GetCurrent().Name))
            {
                return ReservationState.UrlAvailableForThisUser;
            }
            else
            {
                return ReservationState.UrlReservedForSomeoneElse;
            }
#pragma warning restore CA1416 // Validate platform compatibility
        }
        else
        {
            return ReservationState.UrlNotReserved;
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
            UseShellExecute = false,
            RedirectStandardOutput = false,
            RedirectStandardError = false,
            CreateNoWindow = true
        };

        try
        {
            using (var p = Process.Start(psi))
            {
                if (p != null)
                    p.WaitForExit();
                else
                    Console.Error.WriteLine("URL ACL didn't start");
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to add URL ACL for {prefix}: {ex.Message}");
        }
    }

    private static void RemoveUrlAcl(string prefix)
    {
        if (IsNotOnWindows)
            return;
#pragma warning disable CA1416 // Validate platform compatibility; IsNotOnWindows catches this.
        var username = WindowsIdentity.GetCurrent().Name;
#pragma warning restore CA1416 // Validate platform compatibility
        var arguments = $"http delete urlacl url={prefix}";

        var psi = new ProcessStartInfo
        {
            FileName = "netsh",
            Arguments = arguments,
            Verb = "runas", // triggers UAC just for netsh
            UseShellExecute = false,
            RedirectStandardOutput = false,
            RedirectStandardError = false,
            CreateNoWindow = true
        };

        try
        {
            using (var p = Process.Start(psi))
            {
                if (p != null)
                    p.WaitForExit();
                else
                    Console.Error.WriteLine("URL ACL didn't start");
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to remove URL ACL for {prefix}: {ex.Message}");
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

        try
        {
            using (var p = Process.Start(psi))
            {
                if (p != null)
                {
                    p.BeginOutputReadLine();
                    StringBuilder bld = new();
                    p.OutputDataReceived += (s, e) =>
                    {
                        if (e.Data != null) bld.AppendLine(e.Data);
                    };
                    p.WaitForExit();
                    return bld.ToString();
                }
                Console.WriteLine("Couldnt read current URL ACL's");
                return "";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Reading URL ACL's failed due to {0}", ex);
            return "";
        }
    }
}
