using Ejije.Logging;

namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services;

public class UrlAccessGuarantor
{
    private static readonly Log
        PrefixFileLog = Log.Tech("Using {prefixfile} for keeping track of registered http prefixes");

    public static string GetUrlFile()
    {
        var prefixdir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "doodads-prefixes");
        if (!Directory.Exists(prefixdir)) Directory.CreateDirectory(prefixdir);
        var prefixfile = Path.Combine(prefixdir, "prefixes.txt");
        return prefixfile;
    }
    public static void WipeUrlFile()
    {
        File.Delete(GetUrlFile());
    }
    public static void EnsureUrlAcls(IEnumerable<string> prefixes)
    {
        if (!OperatingSystem.IsWindows()) return;
        var prefixfile = GetUrlFile();
        Log.Post(PrefixFileLog, prefixfile);
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

        Log.Post(NetShDump, arguments);

        try
        {
            using (var p = Process.Start(psi))
            {
                if (p != null)
                {
                    p.WaitForExit();
                    if (p.ExitCode == 0)
                    {
                        Log.Post(NetShExit0, arguments, prefix);
                    } else
                    {
                        Log.Post(NetShExitNot0, arguments, prefix);
                    }
                }
                else
                {
                    Log.Post(NetShDidntStart, arguments);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Post(NetShException, ex, arguments, prefix);
        }
    }
    private static readonly Log
        NetShDump = Log.Tech("Running netsh {args}"),
        NetShDidntStart = Log.Fail("Failed to run netsh {args}"),
        NetShException = Log.Fail("Got {exception} while trying to start netsh {args} for {prefix}"),
        NetShExit0 = Log.Cool("Netsh {args} ran successfully for {prefix}"),
        NetShExitNot0 = Log.Fail("Netsh {args} got {exitcode} for {prefix}");
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

        Log.Post(NetShDump, arguments);

        try
        {
            using (var p = Process.Start(psi))
            {
                if (p != null)
                {
                    p.WaitForExit();
                    if (p.ExitCode == 0)
                    {
                        Log.Post(NetShExit0, arguments, prefix);
                    }
                    else
                    {
                        Log.Post(NetShExitNot0, arguments, prefix);
                    }
                }
                else
                {
                    Log.Post(NetShDidntStart, arguments);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Post(NetShException, ex, arguments, prefix);
        }
    }
}
