namespace Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.StreamingUrlEncoded;

public record MultipartHeader(string Name, string? FileName, Dictionary<string, string> Headers)
{
    public static MultipartHeader Parse(Stream stream)
    {
        var headers = HeaderParser.ParseHeaders(stream);
        if (!headers.TryGetValue("content-disposition", out var cd))
            throw new InvalidDataException("Missing Content-Disposition header");

        string name = HeaderParser.ExtractParameter(cd, "name") ??
                      throw new InvalidDataException("Missing name parameter");
        string? filename = HeaderParser.ExtractParameter(cd, "filename");
        return new MultipartHeader(name, filename, headers);
    }
}