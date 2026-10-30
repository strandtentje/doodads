using Ziewaar.RAD.Doodads.CommonComponents.TextTemplating;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;
using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;
using Ziewaar.RAD.Doodads.FormsValidation.Services.UrlEncodedOnly;
using Ziewaar.RAD.Doodads.StandaloneWebserver.Services;
using Ziewaar.RAD.Doodads.FormsValidation.Services.UrlEncodedOnly.HtmlFields;

namespace Ziewaar.RAD.Doodads.Composites;


[Category("Input & Validation")]
[Title("Parse and validate HTML form from template file and redirect after succesful post")]
[Description("""
Works like HTML form and FileTemplate cobined, with a fredir to the action url after
succesful post. Set `hashid`-constant for a fixed hash id. Form action= route will be used
for redirect url, form id will be used for hash id if no `hashid`-constant is provided.
""")]
[ShortNames("redirform")]
public class HtmlFormTemplateRedirect : IService
{
    private readonly HtmlForm form = new HtmlForm();
    private readonly FileTemplate template = new FileTemplate();
    private readonly HttpRedirectFormat redirect = new HttpRedirectFormat();
    private StampedMap CurrentConstants = new(new object());
    private string HashId = "";

    [EventOccasion("When the form was valid")]
    public event CallForInteraction? OnThen;
    [EventOccasion("When the form wasn't valid")]
    public event CallForInteraction? OnElse;
    [EventOccasion("When the template calls for a branch placeholder")]
    public event CallForInteraction? OnPlaceholder;
    [NeverHappens]
    public event CallForInteraction? OnException;
    public HtmlFormTemplateRedirect()
    {
        form.OnThen += (s, e) => template.Enter(CurrentConstants, e);
        form.OnValid += (s, e) =>
        {
            OnThen?.Invoke(this, e);
            if (!e.TryGetCustom<ValidatingInputFieldSet>(out var fs) || fs == null)
                throw new BasicException("missing form fields; this shouldn't happen");
            var finalId = string.IsNullOrWhiteSpace(this.HashId) ? fs.FormId : this.HashId;
            var redirPath = $"{fs.Route}#{finalId}";
            var fredirConstants = new StampedMap(redirPath);
            redirect.Enter(fredirConstants, e);
        };
        form.OnInvalid += (s, e) => OnElse?.Invoke(this, e);
        template.OnElse += (s, e) => OnPlaceholder?.Invoke(this, e);
    }
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        this.CurrentConstants = constants;
        this.HashId = constants.NamedItems.TryGetValue("hashid", out var hashIdObj) &&
            hashIdObj.ToString() is string hashIdCandidate &&
            !string.IsNullOrWhiteSpace(hashIdCandidate) ? hashIdCandidate : "";            
        form.Enter(constants, interaction);
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
