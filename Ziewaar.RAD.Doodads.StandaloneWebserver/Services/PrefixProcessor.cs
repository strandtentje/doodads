using System.Net.Sockets;

namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services;
#pragma warning disable 67
public class PrefixProcessor
{
    private string[]? ActivePrefixes = null;

    public enum ChangeState { Changed, NotChanged, Empty }

    public readonly ExpandedPrefixes ActiveExpandedPrefixes = new();

    public ChangeState TryHandlePrefixChanges(StampedMap constants, IUpdatingValue valueHolder,
        out string[]? newPrefixes)
    {
        object[] replacementPrefixes = ["http://localhost:8243/"];
        if ((constants, valueHolder).IsRereadRequired(out object? newCandidatePrefixes))
        {
            if (newCandidatePrefixes is IEnumerable newCandidatePrefixesArray)
                replacementPrefixes = newCandidatePrefixesArray.OfType<object>().ToArray();
            else if (newCandidatePrefixes?.ToString() is { } newCandidatePrefixesString)
                replacementPrefixes = [newCandidatePrefixesString];
        }

        var replacementPrefixStrings = replacementPrefixes.OfType<string>().Order().ToArray();
        newPrefixes = null;
        if (ActivePrefixes?.SequenceEqual(replacementPrefixStrings) == true)
            return ChangeState.NotChanged;
        if (replacementPrefixStrings.Length == 0)
            return ChangeState.Empty;
        newPrefixes = ActivePrefixes = replacementPrefixStrings;
        foreach (var item in ActivePrefixes)
        {
            ActiveExpandedPrefixes.LoopbackURL = item.Replace("*", "127.0.0.1").Replace("+", "127.0.0.1");
            if (TryGetLocalIpAddress(out string addr))
                ActiveExpandedPrefixes.LocalIPURL = item.Replace("*", addr).Replace("+", addr);
            try
            {
                var hostname = Dns.GetHostName();
                ActiveExpandedPrefixes.LocalHostnameURL = item.Replace("*", hostname).Replace("+", hostname);
            }
            catch (Exception)
            {
                // whatever
            }
        }

        return ChangeState.Changed;
    }

    private static bool TryGetLocalIpAddress(out string addr)
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork && ip.GetAddressBytes().ElementAtOrDefault(0) != 127)
            {
                addr = ip.ToString();
                return true;
            }
        }

        addr = "";
        return false;
    }
}