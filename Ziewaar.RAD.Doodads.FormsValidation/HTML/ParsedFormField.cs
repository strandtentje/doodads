namespace Ziewaar.RAD.Doodads.FormsValidation.HTML;
public class ParsedFormField
{
    public List<string> Options;
    public bool Required, Multiple;
    public int MinLength, MaxLength;
    public string OriginalName, Pattern, MinValue, MaxValue, NameInRequest;
}