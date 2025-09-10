using Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.FormStructure;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic;

[Category("Input & Validation")]
[Title("For an applicable form, attempt to CSRF deobfuscate it before validation")]
[Description("""
             Provided an HtmlFormApplicable (in)directly invoked this, this will 
             dig up the appropriate CSRF session and make the field names normal again.
             """)]
public class HtmlFormDeobfuscate : IService
{
    [EventOccasion("Normalized field names come out here for validating")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [EventOccasion("When there was no form structure or obfuscation info.")]
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
        OnThen?.Invoke(this, new DeobfuscatingFieldNameMappingInteraction(interaction, formStructure, csrfSource.Fields));
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}