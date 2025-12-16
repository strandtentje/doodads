using Ziewaar.RAD.Doodads.CommonComponents.TextTemplating;
#pragma warning disable 67

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.UrlEncodedOnly;

[Category("Input & Validation")]
[Title("Parse and validate HTML form from template file")]
[Description("""
    Works like HTML form and FileTemplate cobined
    """)]
public class HtmlFormTemplate : IService
{
    private readonly HtmlForm form = new HtmlForm();
    private readonly FileTemplate template = new FileTemplate();
    private StampedMap CurrentConstants = new(new object());
    [EventOccasion("When the form was valid")]
    public event CallForInteraction? OnThen;
    [EventOccasion("When the form wasn't valid")]
    public event CallForInteraction? OnElse;
    [EventOccasion("When the template calls for a branch placeholder")]
    public event CallForInteraction? OnPlaceholder;
    [NeverHappens]
    public event CallForInteraction? OnException;
    public HtmlFormTemplate()
    {
        form.OnThen += (s, e) => template.Enter(CurrentConstants, e);
        form.OnValid += (s, e) => OnThen?.Invoke(this, e);
        form.OnInvalid += (s, e) => OnElse?.Invoke(this, e);
        template.OnElse += (s, e) => OnPlaceholder?.Invoke(this, e);
    }
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        this.CurrentConstants = constants;
        form.Enter(constants, interaction);
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
