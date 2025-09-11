namespace Ziewaar.RAD.Doodads.Testkit;
public record MultipartMember(string Name, string? TextOrFilename, string? ContentType, FileInfo? File)
{
    public static IList<MultipartMember> Multiple() => new List<MultipartMember>();
}