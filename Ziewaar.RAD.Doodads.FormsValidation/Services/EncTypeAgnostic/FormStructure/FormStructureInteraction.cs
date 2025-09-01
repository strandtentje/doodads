namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.FormStructure;

public class FormStructureInteraction(
    IInteraction stack,
    string requestContentType,
    string responseContentType,
    HttpMethod httpMethod,
    string actionUrl,
    HtmlNode formNode,
    List<FormStructureMember> members) : IInteraction, IFieldNameMappingInteraction
{
    public IInteraction Stack => stack;
    public object Register => stack.Register;
    public IReadOnlyDictionary<string, object> Memory => stack.Memory;
    public bool TryGetRealNameOf(string incomingName, out string? realName)
    {
        realName = incomingName;
        return true;
    }
    public string RequestContentType => requestContentType;
    public string ResponseContentType => responseContentType;
    public HttpMethod HttpMethod => httpMethod;
    public string ActionUrl => actionUrl;
    public HtmlNode FormNode => formNode;
    public List<FormStructureMember> Members => members;
    public static FormStructureInteractionBuilder Builder => new();

    public bool AppliesTo(HttpMethod parsedMethod, string candidateUrl, string incomingContentType) =>
        this.HttpMethod == parsedMethod && this.ActionUrl == candidateUrl &&
        this.RequestContentType == incomingContentType;

    public string GetName() => $"{RequestContentType} {HttpMethod} {ActionUrl}";
}