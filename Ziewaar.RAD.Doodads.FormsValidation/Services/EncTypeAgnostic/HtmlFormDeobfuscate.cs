using Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.FormStructure;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic;

public class HtmlFormDeobfuscate : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
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