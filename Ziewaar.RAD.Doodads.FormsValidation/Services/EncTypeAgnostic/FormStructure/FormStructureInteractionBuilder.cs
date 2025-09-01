namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.FormStructure;
public class FormStructureInteractionBuilder
{
    public bool IsSealed { get; private set; } = false;
    private string RequestContentType = "application/x-www-form-urlencoded";
    private HttpMethod HttpMethod = HttpMethod.Get;
    private string ActionUrl = "";
    private List<FormStructureMember> Members = new();
    private HtmlNode FormNode;
    private string ResponseContentType = "text/html";

    public FormStructureInteractionBuilder WithResponseBodyType(string contentType)
    {
        if (IsSealed) throw new InvalidOperationException("Cannot change sealed formstructure builder");
        this.ResponseContentType = contentType;
        return this;
    }
    public FormStructureInteractionBuilder WithRequestBodyType(string contentType)
    {
        if (IsSealed) throw new InvalidOperationException("Cannot change sealed formstructure builder");
        this.RequestContentType = contentType;
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
            this.RequestContentType, this.ResponseContentType, this.HttpMethod, this.ActionUrl, this.FormNode, this.Members);
    }

    public FormStructureInteractionBuilder WithHtmlForm(HtmlNode formNode)
    {
        if (IsSealed) throw new InvalidOperationException("Cannot set form html on sealed form data");
        this.FormNode = formNode;
        return this;
    }
}