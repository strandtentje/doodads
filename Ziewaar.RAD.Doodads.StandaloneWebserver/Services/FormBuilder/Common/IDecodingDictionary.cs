namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services;
#pragma warning disable 67
public interface IDecodingDictionary : IReadOnlyDictionary<string, object>
{
    static abstract IDecodingDictionary CreateFor(string encodedData, string[] unsanitizedWhitelist);
}