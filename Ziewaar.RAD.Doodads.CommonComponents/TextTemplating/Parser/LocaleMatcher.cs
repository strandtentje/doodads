using System.Threading;

namespace Ziewaar.RAD.Doodads.CommonComponents.TextTemplating.Parser;
public static class LocaleMatcher
{
    /// <summary>
    /// Determines if a demanded and provided locale match, considering wildcards on either side.
    /// Locales are expected in the format "ll_cc" (language_country) or with wildcards ("en_*", "*_US", etc).
    /// </summary>
    public static bool IsLocaleMatch(string demanded, string provided)
    {
        // Normalize to lowercase and handle '-' and '_' interchangeably
        string Normalize(string loc) => loc.Replace('-', '_').ToLowerInvariant();

        var d = Normalize(demanded).Split('_');
        var p = Normalize(provided).Split('_');

        // Pad with wildcards if missing
        if (d.Length == 1) Array.Resize(ref d, 2); // "en" → "en", null
        if (p.Length == 1) Array.Resize(ref p, 2);

        // Fill nulls with "*"
        d[0] = d[0] ?? "*";
        d[1] = (d.Length > 1 && d[1] != null) ? d[1] : "*";
        p[0] = p[0] ?? "*";
        p[1] = (p.Length > 1 && p[1] != null) ? p[1] : "*";

        // Returns true if one string matches the other, or either side is a "*"
        bool Match(string a, string b) => a == "*" || b == "*" || a == b;

        // Check both directions:
        // Demanded is satisfied by provided, OR provided is satisfied by demanded
        bool DirectionalMatch(string[] lhs, string[] rhs)
            => Match(lhs[0], rhs[0]) && Match(lhs[1], rhs[1]);

        return DirectionalMatch(d, p) || DirectionalMatch(p, d);
    }
}