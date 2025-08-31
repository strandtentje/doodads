using Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.FormStructure;
using Ziewaar.RAD.Doodads.FormsValidation.Services.Support;
using Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.Readers;
using Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.StreamingUrlEncoded;
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
                .WithContentType(formNode.GetAttributeValue("enctype", "application/x-www-form-urlencoded"))
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
public class HtmlFormApplicable : IService
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

        if (!interaction.TryFindVariable("method", out string? candidateMethod) ||
            candidateMethod == null ||
            !interaction.TryFindVariable("url", out string? candiateUrl) ||
            candiateUrl == null)
        {
            OnException?.Invoke(this,
                new CommonInteraction(interaction,
                    "At least method and URL should be known to find out if the html form is applicable"));
            return;
        }

        var parsedMethod = HttpMethod.Parse(candidateMethod);

        string incomingContentType = "application/x-www-form-urlencoded";
        IEnumerable<IGrouping<string, object>> formData;

        if (parsedMethod == HttpMethod.Get)
        {
            if (formStructure.AppliesTo(parsedMethod, candiateUrl, incomingContentType) &&
                interaction.TryFindVariable("query", out string? queryString) && queryString != null)
            {
                formData = new FormDataDictionary(queryString)
                    .SelectMany(x => x.Value.OfType<object>().Select(value => (key: x.Key, value)))
                    .GroupBy(x => x.key, x => x.value);
                OnThen?.Invoke(this, new FormDataInteraction(interaction, formStructure, formData));
                return;
            }
            else
            {
                OnElse?.Invoke(this, interaction);
                return;
            }
        }

        if (!interaction.TryGetClosest<ISourcingInteraction>(out var formDataSource) || formDataSource == null)
        {
            OnException?.Invoke(this,
                new CommonInteraction(interaction, "Non-GET body received; expecting sourcing interaction"));
            return;
        }

        incomingContentType = formDataSource.SourceContentTypePattern;

        if (!formStructure.AppliesTo(parsedMethod, candidateMethod, incomingContentType))
        {
            OnElse?.Invoke(this, interaction);
            return;
        }

        if (incomingContentType == "application/x-www-form-urlencoded")
        {
            var byteReader = new RootByteReader(formDataSource.SourceBuffer);
            var urlEncodedReader = new UrlEncodedTokenReader(byteReader);
            formData = new LazyFormDataDictionary(urlEncodedReader);
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
public class FormDataInteraction(
    IInteraction interaction,
    FormStructureInteraction structure,
    IEnumerable<IGrouping<string, object>> data) : IInteraction
{
    public IInteraction Stack { get; }
    public object Register { get; }
    public IReadOnlyDictionary<string, object> Memory { get; }
}