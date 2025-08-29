namespace Ziewaar.RAD.Doodads.FormsValidation.Services.HtmlRevisited;
public class FormStructureInteraction(
    IInteraction stack, string contentType, 
    HttpMethod httpMethod, string actionUrl,
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
}