namespace Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;

public static class ContentTypeMatcher
{
    public static bool IsMatch(string a, string b)
    {
        var ctA = Parse(a);
        var ctB = Parse(b);

        // Try matching A against B pattern
        if (OneWayMatch(ctA, ctB))
            return true;

        // Or B against A pattern
        return OneWayMatch(ctB, ctA);
    }

    private static bool OneWayMatch(ContentType pattern, ContentType value)
    {
        if (!WildcardMatch(pattern.Type, value.Type)) return false;
        if (!WildcardMatch(pattern.Subtype, value.Subtype)) return false;

        // If the pattern has no parameters, it's a match regardless of value parameters.
        if (pattern.Parameters.Count == 0) return true;

        // Otherwise, value must have all the same parameters with the same values (case-insensitive)
        foreach (var kvp in pattern.Parameters)
        {
            if (!value.Parameters.TryGetValue(kvp.Key, out var val)) return false;
            if (!string.Equals(kvp.Value, val, StringComparison.OrdinalIgnoreCase)) return false;
        }

        return true;
    }

    private static bool WildcardMatch(string patternPart, string valuePart)
    {
        return patternPart == "*" || string.Equals(patternPart, valuePart, StringComparison.OrdinalIgnoreCase);
    }

    private static ContentType Parse(string input)
    {
        var parts = input.Split(';');
        var types = parts[0].Trim().Split('/');
        var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (types.Length != 2)
            throw new FormatException($"Invalid media type: {input}");

        for (int i = 1; i < parts.Length; i++)
        {
            var param = parts[i].Trim();
            var kv = param.Split(new[] { '=' }, 2);
            if (kv.Length == 2)
                parameters[kv[0].Trim()] = kv[1].Trim().Trim('"'); // handle quoted values
        }

        return new ContentType
        {
            Type = types[0].Trim(),
            Subtype = types[1].Trim(),
            Parameters = parameters
        };
    }

    private class ContentType
    {
        public string Type { get; set; }
        public string Subtype { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
    }
}