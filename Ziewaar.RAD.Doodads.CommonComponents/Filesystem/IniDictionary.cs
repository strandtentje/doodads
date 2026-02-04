#nullable enable
#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.CommonComponents.Filesystem;

public static class IniDictionary
{
    internal static IReadOnlyDictionary<string, object> FromFile(string registerPath)
    {
        if (!File.Exists(registerPath))
            return EmptyReadOnlyDictionary.Instance;
        var lines = File.ReadAllLines(registerPath);
        var pairs = lines.
            Select(x => x.Split(['='], 2, StringSplitOptions.RemoveEmptyEntries)).
            Where(x => x.Length == 2).
            Select(x => x.Select(y => y.Trim()).ToArray()).
            ToDictionary(x => x[0], x => (object)x[1], StringComparer.OrdinalIgnoreCase);
        return pairs;
    }
}