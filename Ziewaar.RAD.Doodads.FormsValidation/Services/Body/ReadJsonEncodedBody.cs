namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services;
#pragma warning disable 67

[Category("HTTP Forms")]
[Title("JSON Body Reading")]
[Description("""
             Reads the top level JSON values into the memory based on whitelist or csrf lookup.
             """)]
public class ReadJsonEncodedBody : ReadEncodedBody<JsonDecodingQueryDictionary>;