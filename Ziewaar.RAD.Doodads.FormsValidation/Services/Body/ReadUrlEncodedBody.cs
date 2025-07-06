using Ziewaar.RAD.Doodads.FormsValidation.Common;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.Body;
#pragma warning disable 67

[Category("HTTP Forms")]
[Title("URL Encoded Body Reading")]
[Description("""
             Reads URL Encoded Form values into the memory based on whitelist or csrf lookup.
             May be used with URL Query string, or POST body.
             """)]
public class ReadUrlEncodedBody : ReadEncodedBody<UrlDecodingQueryDictionary>;