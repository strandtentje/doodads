namespace Ziewaar.RAD.Doodads.FormsValidation.Services.Body;
#pragma warning disable 67

[Category("Input & Validation")]
[Title("URL Encoded Body Reading")]
[Description("""
             Reads URL Encoded Form values into the memory based on whitelist or csrf lookup.
             May be used with URL Query string, or POST body.
             """)]
public class ReadUrlEncodedBody : ReadEncodedBody<UrlDecodingQueryDictionary>;