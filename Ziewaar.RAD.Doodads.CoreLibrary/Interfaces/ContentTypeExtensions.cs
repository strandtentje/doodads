namespace Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
#nullable enable
public static class ContentTypeExtensions
{
    /// <summary>
    /// Extracts the base MIME type from a full content-type string, stripping off parameters.
    /// </summary>
    public static string GetBaseHeader(this string? contentType, string fallback = "*/*")
    {
        if (string.IsNullOrWhiteSpace(contentType) || contentType == null)
            return fallback;

        int semicolonIndex = contentType.IndexOf(';');
        return semicolonIndex >= 0
            ? contentType.Substring(0,semicolonIndex).Trim()
            : contentType.Trim();
    }

    /// <summary>
    /// Parses the parameters from a content-type string into a dictionary.
    /// </summary>
    public static IReadOnlyDictionary<string, string> GetHeaderProperties(this string? headerValue)
    {
        if (string.IsNullOrWhiteSpace(headerValue) || headerValue == null)
            return new SortedList<string, string>(0);

        var properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        int semicolonIndex = headerValue.IndexOf(';');
        if (semicolonIndex < 0)
            return properties;

        var segments = headerValue.Substring(semicolonIndex + 1)
            .Split(';').Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToArray();

        foreach (var segment in segments)
        {
            var kvp = segment.Split(['='], 2);
            if (kvp.Length == 2)
            {
                var key = kvp[0].Trim();
                var value = kvp[1].Trim().Trim('"'); // remove surrounding quotes if present
                if (!string.IsNullOrEmpty(key))
                    properties[key] = value;
            }
        }

        return new ReadOnlyDictionary<string, string>(properties);
    }
}