using System.Xml.Resolvers;
using Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.FormStructure;
using Ziewaar.RAD.Doodads.FormsValidation.Services.Support;
using Ziewaar.RAD.Doodads.FormsValidation.Services.UrlEncodedOnly;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic;

[Category("Input & Validation")]
[Title("Attempts to prepare for form validation based on an HTML form.")]
[Description("""
             Will parse any standard HTML form into a set of relatively sane validation rules.
             HtmlFormValidate, HtmlFormApplicable and HtmlFormPrint(Obfuscated) rely on this 
             service to have worked.
             """)]
public class HtmlFormPrepare : IService
{
    [EventOccasion("When the form is prepared and ready to use.")]
    public event CallForInteraction? OnThen;
    [EventOccasion("Sink the HTML form body here using ie. FileTemplate")]
    public event CallForInteraction? OnElse;
    [EventOccasion("When there was no (HTTP) sink to mimic for the HTML form source.")]
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

            var encType = formNode.GetAttributeValue("enctype", "application/x-www-form-urlencoded");
            var isMultipart = encType.Equals("multipart/form-data", StringComparison.OrdinalIgnoreCase);
            string action = formNode.GetAttributeValue("action", "");
            
            IEnumerable<char> getActionBeforeSpecial()
            {
                using (var e = action.GetEnumerator())
                {
                    while(e.MoveNext() && e.Current != '#' && e.Current != '?')
                        yield return e.Current;
                }
            }

            CurrentFormBuilder = FormStructureInteraction.Builder
                .WithHtmlForm(formNode)
                .WithRequestBodyType(encType)
                .WithMethod(formNode.GetAttributeValue("method", "GET"))
                .WithAction(new string(getActionBeforeSpecial().ToArray()));

            var inputGroups = formNode.SelectNodes(InputElementFilter)
                .NotDisabled()
                .Select(inputNode => (inputName: inputNode.GetAttributeValue("name", string.Empty), node: inputNode))
                .Where(tuple => !string.IsNullOrWhiteSpace(tuple.inputName))
                .GroupBy(x => x.inputName, x => x.node);

            foreach (IGrouping<string, HtmlNode> inputGroup in inputGroups)
            {
                var classes = inputGroup.GetInputClasses();
                var isFileTag = classes.Any(x => x.Type.Equals("file", StringComparison.OrdinalIgnoreCase));
                if (isFileTag && !isMultipart)
                {
                    OnException?.Invoke(this,
                        new CommonInteraction(interaction, "may only put file fields in enctype multipart/form-data"));
                    return;
                }

                CurrentFormBuilder.Add(
                    FormStructureMember.Builder
                        .SetName(inputGroup.Key)
                        .SetTypes(classes)
                        .AddAccepts(inputGroup.GetAcceptAttributes())
                        .SetOptions(inputGroup.GetValidLiteralValues())
                        .CanOnlyFitOptions(inputGroup.IsOptionType())
                        .SetLengthBounds(inputGroup.GetMinLength(), inputGroup.GetMaxLength())
                        .SetValueBounds(inputGroup.GetMinValues(), inputGroup.GetMaxValues())
                        .SetPatternConstraints(inputGroup.GetPatterns())
                        .SetValueCountLimits(
                            inputGroup.GetMinExpectedValueCount(),
                            inputGroup.GetMaxExpectedValueCount())
                        .Build(isMultipart, isFileTag));
            }

            CurrentFormBuilder.Seal();
        }

        OnThen?.Invoke(this, CurrentFormBuilder.CreateFor(interaction));
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}