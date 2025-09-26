#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.Generate;

public static class DataUrlUtil
{
    public static string ToBase64DataUrl(Stream stream, string contentType)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));
        if (string.IsNullOrWhiteSpace(contentType))
            throw new ArgumentException("Content type must be provided.", nameof(contentType));

        using (var memoryStream = new MemoryStream())
        {
            stream.CopyTo(memoryStream);
            var base64 = Convert.ToBase64String(memoryStream.ToArray());
            return $"data:{contentType};base64,{base64}";
        }
    }
}
