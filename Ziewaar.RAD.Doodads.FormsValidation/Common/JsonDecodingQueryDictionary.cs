using Newtonsoft.Json;

namespace Ziewaar.RAD.Doodads.FormsValidation.Common;
#pragma warning disable 67
public class JsonDecodingQueryDictionary(
    string[] jsonSanitizedWhitelist,
    string jsonEncodedQuery) :
    LazyDictionary(jsonSanitizedWhitelist), IDecodingDictionary
{
    private Dictionary<string, object>? BackingStore;
    public static IDecodingDictionary CreateFor(string encodedData, string[] unsanitizedWhitelist) =>
        new JsonDecodingQueryDictionary(
            unsanitizedWhitelist.Select(JsonConvert.ToString).ToArray(),
            encodedData);
    protected override object? FindValue(string sanitizedKey)
    {
        this.BackingStore ??= JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonEncodedQuery);
        return this.BackingStore?.GetValueOrDefault(sanitizedKey);
    }
    protected override string Sanitize(string unsanitized) => unsanitized;
    protected override string Desanitize(string sanitized) => sanitized;
}