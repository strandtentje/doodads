namespace Ziewaar.RAD.Doodads.FormsValidation.Common;
#pragma warning disable 67
public interface IDecodingDictionary : IReadOnlyDictionary<string, object>
{
    static abstract IDecodingDictionary CreateFor(string encodedData, string[] unsanitizedWhitelist);
}