using Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.FormStructure;
namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic;
#pragma warning disable 67

[Category("Input & Validation")]
[Title("Prints a prepared form to the true output.")]
[Description("""
             After HtmlFormPrepare happened, this may be invoked at any point to print the form
             to the current sink; likely an HTTP request.
             """)]
public class HtmlFormPrint : IService
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
        if (!interaction.TryGetClosest<ISinkingInteraction>(out var sinkingSource) || sinkingSource == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Sink required to write obfuscated html form"));
            return;
        }

        using var ct = sinkingSource.GetWriter(formStructure.ResponseContentType);
        formStructure.FormNode.WriteTo(ct);
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}