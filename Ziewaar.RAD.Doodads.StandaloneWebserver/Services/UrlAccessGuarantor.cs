namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services;

public class UrlAccessGuarantor
{
    public static void EnsureUrlAcls(IEnumerable<string> prefixes)
    {
        if (!OperatingSystem.IsWindows()) return;
        
        var prefixdir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "doodads-prefixes");
        if (!Directory.Exists(prefixdir)) Directory.CreateDirectory(prefixdir);
        var prefixfile = Path.Combine(prefixdir, "prefixes.txt");
        Console.WriteLine($"using {prefixfile} for keeping track of registered prefixes");
        if (!File.Exists(prefixfile)) File.WriteAllText(prefixfile, "");
        var prefixLines = File.ReadAllLines(prefixfile).Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

        foreach (var prefix in prefixes)
        {
            if (!prefixLines.Contains(prefix))
            {
                RemoveUrlAcl(prefix);
                AddUrlAcl(prefix);
                File.AppendAllLines(prefixfile, [prefix]);
            }
        }
    }

    private static void AddUrlAcl(string prefix)
    {
        if (!OperatingSystem.IsWindows()) return;
        
        var everyoneSid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
        var ntAccount = (NTAccount)everyoneSid.Translate(typeof(NTAccount));
        
        var arguments = $"http add urlacl url={prefix} user={ntAccount.Value}";

        var psi = new ProcessStartInfo
        {
            FileName = "netsh",
            Arguments = arguments,
            Verb = "runas", // triggers UAC just for netsh
            UseShellExecute = true,
            WindowStyle = ProcessWindowStyle.Normal,
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
        if (!OperatingSystem.IsWindows()) return;
        var arguments = $"http delete urlacl url={prefix}";

        var psi = new ProcessStartInfo
        {
            FileName = "netsh",
            Arguments = arguments,
            Verb = "runas", // triggers UAC just for netsh
            UseShellExecute = true,
            WindowStyle = ProcessWindowStyle.Normal,
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
}
