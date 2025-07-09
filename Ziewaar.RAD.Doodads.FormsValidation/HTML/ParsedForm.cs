namespace Ziewaar.RAD.Doodads.FormsValidation.HTML;
public class ParsedForm
{
    public string FullRoute;
    public HttpMethod Method;
    public List<ParsedFormField> Fields = new();
}