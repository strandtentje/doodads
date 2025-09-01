using System.Text;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.StreamingUrlEncoded;

public static class MultipartHeaderExtensions
{
    public static Encoding GetEncodingOrDefault(this IReadOnlyDictionary<string, string> headers, Encoding fallback)
    {
        if (headers.TryGetValue("content-type", out var contentType))
        {
            var charset = ExtractCharset(contentType);
            if (!string.IsNullOrEmpty(charset))
            {
                try
                {
                    return Encoding.GetEncoding(charset);
                }
                catch (ArgumentException) { }
                catch (NotSupportedException) { }
            }
        }

        return fallback;
    }

    private static string? ExtractCharset(string contentType)
    {
        var parts = contentType.Split(';');
        foreach (var part in parts)
        {
            var trimmed = part.Trim();
            if (trimmed.StartsWith("charset=", StringComparison.OrdinalIgnoreCase))
            {
                return trimmed.Substring("charset=".Length).Trim('\"');
            }
        }

        return null;
    }
}