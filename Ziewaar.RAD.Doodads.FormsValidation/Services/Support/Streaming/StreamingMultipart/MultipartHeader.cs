namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic;
public record MultipartHeader(string HeaderName, string HeaderValue, IReadOnlyDictionary<string, string> HeaderArgs);