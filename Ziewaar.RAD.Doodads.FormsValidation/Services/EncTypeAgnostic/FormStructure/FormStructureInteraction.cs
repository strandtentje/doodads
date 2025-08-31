namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.FormStructure;
public class FormStructureInteraction(
    IInteraction stack,
    string contentType,
    HttpMethod httpMethod,
    string actionUrl,
    List<FormStructureMember> members) : IInteraction
{
    public IInteraction Stack => stack;
    public object Register => stack.Register;
    public IReadOnlyDictionary<string, object> Memory => stack.Memory;
    public string ContentType => contentType;
    public HttpMethod HttpMethod => httpMethod;
    public string ActionUrl => actionUrl;
    public List<FormStructureMember> Members => members;
    public static FormStructureInteractionBuilder Builder => new();
    public bool AppliesTo(HttpMethod parsedMethod, string candidateUrl, string incomingContentType) =>
        this.HttpMethod == parsedMethod && this.ActionUrl == candidateUrl &&
        this.ContentType == incomingContentType;
}