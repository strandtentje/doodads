using Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.FormStructure;
using Ziewaar.RAD.Doodads.FormsValidation.Services.Support;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic;

public class HtmlFormPrintObfuscated : IService
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