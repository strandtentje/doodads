using Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.FormStructure;
using Ziewaar.RAD.Doodads.FormsValidation.Services.Support;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic;

[Category("Input & Validation")]
[Title("Prints a prepared and obfuscated form to the true otuput.")]
[Description("""
             After HtmlFormPrepare happened, this may be invoked at any point to print the form
             to the current sink; likely an HTTP request. It'll obfuscate the field names with one-time 
             secrets for CSRF protection.
             """)]
public class HtmlFormPrintObfuscated : IService
{
    [NeverHappens]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [EventOccasion("When HtmlFormPrepare didn't previously happen, or there was no sane place to sink the form into.")]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (!interaction.TryGetClosest<FormStructureInteraction>(out var formStructure) || formStructure == null)
        {
            OnException?.Invoke(this,
                new CommonInteraction(interaction, "Form structure required for this; use ie. HtmlFormPrepare"));
            return;
        }
        if (!interaction.TryGetClosest<ICsrfTokenSourceInteraction>(out var csrfSource) || csrfSource == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Obfuscation requires CSRF to be setup with CsrfFields"));
            return;
        }
        if (!interaction.TryGetClosest<ISinkingInteraction>(out var sinkingSource) || sinkingSource == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Sink required to write obfuscated html form"));
            return;
        }

        var obfuscationMap = formStructure.Members.ToDictionary(x => x.Name,
            x => csrfSource.Fields.NewObfuscation(formStructure.GetName(), x.Name));

        using var ct = sinkingSource.GetWriter(formStructure.ResponseContentType);
        formStructure.FormNode.RemapValuesForAttributes(["name", "for"], obfuscationMap).WriteTo(ct);
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}