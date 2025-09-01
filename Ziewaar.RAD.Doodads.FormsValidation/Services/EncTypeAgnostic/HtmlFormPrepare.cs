using System.Xml.Resolvers;
using Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.FormStructure;
using Ziewaar.RAD.Doodads.FormsValidation.Services.Support;
using Ziewaar.RAD.Doodads.FormsValidation.Services.UrlEncodedOnly;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic;

public class HtmlFormPrepare : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    private FormStructureInteractionBuilder? CurrentFormBuilder = null;
    private TextSinkingInteraction? CurrentHtmlSink = null;
    private static readonly string[] InputElementNames = ["input", "select", "textarea", "button"];
    private static string InputElementFilter => string.Join('|', InputElementNames.Select(x => $".//{x}"));

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (!interaction.TryGetClosest<ISinkingInteraction>(out var output) || output == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "No sink found to template into."));
            return;
        }

        if (CurrentFormBuilder != null && CurrentHtmlSink != null)
        {
            var probeForUpdate = new CheckUpdateRequiredInteraction(interaction, CurrentHtmlSink);
            OnElse?.Invoke(this, probeForUpdate);
            if (probeForUpdate.IsRequired)
            {
                CurrentHtmlSink?.SinkBuffer.Dispose();
                CurrentHtmlSink = null;
                CurrentFormBuilder = null;
            }
        }

        if (CurrentFormBuilder == null)
        {
            CurrentHtmlSink = TextSinkingInteraction.CreateIntermediateFor(output, interaction);
            OnElse?.Invoke(this, CurrentHtmlSink);
            HtmlDocument fullHtmlDocument = new();
            CurrentHtmlSink.SinkBuffer.Position = 0;
            fullHtmlDocument.Load(CurrentHtmlSink.SinkBuffer, CurrentHtmlSink.TextEncoding);

            if (fullHtmlDocument.DocumentNode.SelectSingleNode("//form") is not HtmlNode formNode)
            {
                OnException?.Invoke(this, new CommonInteraction(interaction, "missing form node"));
                return;
            }

            CurrentFormBuilder = FormStructureInteraction.Builder
                .WithHtmlForm(formNode)
                .WithRequestBodyType(formNode.GetAttributeValue("enctype", "application/x-www-form-urlencoded"))
                .WithMethod(formNode.GetAttributeValue("method", "GET"))
                .WithAction(formNode.GetAttributeValue("action", ""));

            var inputGroups = formNode.SelectNodes(InputElementFilter)
                .NotDisabled()
                .Select(inputNode => (inputName: inputNode.GetAttributeValue("name", string.Empty), node: inputNode))
                .Where(tuple => !string.IsNullOrWhiteSpace(tuple.inputName))
                .GroupBy(x => x.inputName, x => x.node);

            foreach (IGrouping<string, HtmlNode> inputGroup in inputGroups)
                CurrentFormBuilder.Add(
                    FormStructureMember.Builder
                        .SetName(inputGroup.Key)
                        .SetTypes(inputGroup.GetInputClasses())
                        .SetOptions(inputGroup.GetValidLiteralValues())
                        .SetLengthBounds(inputGroup.GetMinLength(), inputGroup.GetMaxLength())
                        .SetValueBounds(inputGroup.GetMinValues(), inputGroup.GetMaxValues())
                        .SetPatternConstraints(inputGroup.GetPatterns())
                        .SetValueCountLimits(
                            inputGroup.GetMinExpectedValueCount(),
                            inputGroup.GetMaxExpectedValueCount())
                        .Build());
            CurrentFormBuilder.Seal();
        }

        OnThen?.Invoke(this, CurrentFormBuilder.CreateFor(interaction));
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}