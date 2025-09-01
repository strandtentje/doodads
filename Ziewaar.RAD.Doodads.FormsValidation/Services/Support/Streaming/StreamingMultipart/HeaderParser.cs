using System.Text;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.StreamingUrlEncoded;

public static class HeaderParser
{
    public static Dictionary<string, string> ParseHeaders(Stream stream)
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        using var reader = new StreamReader(stream, Encoding.ASCII, leaveOpen: true);

        const int maxHeaderLines = 750;
        const int maxLineLength = 8192;
        const int maxTotalHeaderBytes = 128 * 1024;

        string? currentHeaderLine;
        string? currentHeaderKey = null;
        var sb = new StringBuilder();
        int totalBytes = 0;
        int linesRead = 0;

        // 🧼 Skip blank lines before headers
        do
        {
            currentHeaderLine = reader.ReadLine();
        } while (string.IsNullOrEmpty(currentHeaderLine));

        while (!string.IsNullOrEmpty(currentHeaderLine))
        {
            linesRead++;
            totalBytes += currentHeaderLine.Length;

            if (linesRead > maxHeaderLines)
                throw new InvalidDataException("Too many header lines");

            if (currentHeaderLine.Length > maxLineLength)
                throw new InvalidDataException("Header line too long");

            if (totalBytes > maxTotalHeaderBytes)
                throw new InvalidDataException("Header section too large");

            if (char.IsWhiteSpace(currentHeaderLine[0]) && currentHeaderKey != null)
            {
                sb.Append(' ').Append(currentHeaderLine.Trim());
            }
            else
            {
                if (currentHeaderKey != null)
                    headers[currentHeaderKey] = sb.ToString();

                var colonIndex = currentHeaderLine.IndexOf(':');
                if (colonIndex == -1)
                    throw new InvalidDataException("Malformed header: " + currentHeaderLine);

                currentHeaderKey = currentHeaderLine[..colonIndex].Trim();
                sb.Clear();
                sb.Append(currentHeaderLine[(colonIndex + 1)..].Trim());
            }

            currentHeaderLine = reader.ReadLine();
        }

        if (currentHeaderKey != null)
            headers[currentHeaderKey] = sb.ToString();

        return headers;
    }


    public static string? ExtractParameter(string headerValue, string parameterName) =>
        headerValue.Split(';').Select(x => x.Trim())
            .Where(candidateHeaderParameter =>
                candidateHeaderParameter.StartsWith($"{parameterName}=", StringComparison.OrdinalIgnoreCase))
            .Select(keyValueParameter => keyValueParameter[(parameterName.Length + 1)..].Trim())
            .Select(headerParameterValue => headerParameterValue.StartsWith('"') && headerParameterValue.EndsWith('"')
                ? headerParameterValue[1..^1]
                : headerParameterValue).FirstOrDefault();
}