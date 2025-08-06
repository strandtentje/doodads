using Ziewaar.RAD.Doodads.FormsValidation.Common;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.Body;
#pragma warning disable 67

[Category("Input & Validation")]
[Title("JSON Body Reading")]
[Description("""
             Reads the top level JSON values into the memory based on whitelist or csrf lookup.
             """)]
public class ReadJsonEncodedBody : ReadEncodedBody<JsonDecodingQueryDictionary>;