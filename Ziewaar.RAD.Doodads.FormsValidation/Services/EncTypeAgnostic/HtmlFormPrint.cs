using Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.FormStructure;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic;

public class HtmlFormPrint : IService
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