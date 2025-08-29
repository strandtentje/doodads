namespace Ziewaar.RAD.Doodads.FormsValidation.Services.HtmlRevisited;
public class FormStructureInteractionBuilder
{
    public bool IsSealed { get; private set; } = false;
    private string ContentType = "application/x-www-form-urlencoded";
    private HttpMethod HttpMethod = HttpMethod.Get;
    private string ActionUrl = "";
    private List<FormStructureMember> Members = new();
    public FormStructureInteractionBuilder WithContentType(string contentType)
    {
        if (IsSealed) throw new InvalidOperationException("Cannot change sealed formstructure builder");
        this.ContentType = contentType;
        return this;
    }
    public FormStructureInteractionBuilder WithMethod(string method)
    {
        if (IsSealed) throw new InvalidOperationException("Cannot change sealed formstructure builder");
        this.HttpMethod = HttpMethod.Parse(method);
        return this;
    }
    public FormStructureInteractionBuilder WithAction(string actionUrl)
    {
        if (IsSealed) throw new InvalidOperationException("Cannot change sealed formstructure builder");
        this.ActionUrl = actionUrl;
        return this;
    }
    public FormStructureInteractionBuilder Add(FormStructureMember member)
    {
        if (IsSealed) throw new InvalidOperationException("Cannot change sealed formstructure builder");
        this.Members.Add(member);
        return this;
    }
    public void Seal()
    {
        if (IsSealed) throw new InvalidOperationException("Cannot change sealed formstructure builder");
        IsSealed = true;
    }
    public FormStructureInteraction CreateFor(IInteraction stack)
    {
        if (!IsSealed) throw new InvalidOperationException("Cannot create with unsealed formstructure builder");
        return new FormStructureInteraction(
            stack,
            this.ContentType, this.HttpMethod, this.ActionUrl, this.Members);
    }
}