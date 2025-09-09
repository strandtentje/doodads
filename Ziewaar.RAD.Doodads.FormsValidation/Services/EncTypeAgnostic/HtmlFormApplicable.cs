using System.Text;
using Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.FormStructure;
using Ziewaar.RAD.Doodads.FormsValidation.Services.Support;
using Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.Readers;
using Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.StreamingUrlEncoded;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic;

public class HtmlFormApplicable : IService
{
    private readonly UpdatingKeyValue MaxBytesConstant = new("maxlength");
    private readonly UpdatingKeyValue DisableContentLengthRequirementConstant = new("requirecontentlength");
    private long CurrentByteLimit = 2048;
    private bool RequireContentLength = true;
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnProgress;
    public event CallForInteraction? OnRejection;
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, MaxBytesConstant).IsRereadRequired(out object? maxCandidate))
        {
            if (maxCandidate == null)
                this.CurrentByteLimit = 2048;
            else if (maxCandidate is decimal byteCount)
                this.CurrentByteLimit = (long)byteCount;
            else if (maxCandidate is string byteExpression)
                this.CurrentByteLimit = byteExpression.ParseByteSize();
        }

        if ((constants, DisableContentLengthRequirementConstant).IsRereadRequired(
                out bool requireContentLengthCandidate))
            this.RequireContentLength = requireContentLengthCandidate;

        if (!interaction.TryGetClosest<FormStructureInteraction>(out var formStructure) || formStructure == null)
        {
            OnException?.Invoke(this,
                new CommonInteraction(interaction, "Form structure required for this; use ie. HtmlFormPrepare"));
            return;
        }

        if (!interaction.TryFindVariable("method", out string? candidateMethod) ||
            candidateMethod == null ||
            !interaction.TryFindVariable("url", out string? candidateUrl) ||
            candidateUrl == null)
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
            if (formStructure.AppliesTo(parsedMethod, candidateUrl, incomingContentType) &&
                interaction.TryFindVariable("query", out string? queryString) && queryString != null)
            {
                if (queryString.Length > CurrentByteLimit)
                {
                    OnRejection?.Invoke(this, interaction);
                    GlobalLog.Instance?.Debug(
                        "URL Query formdata too long; configure maxlength if this shouldn't be happening");
                    return;
                }
                else
                {
                    formData = new FormDataDictionary(queryString)
                        .SelectMany(x => x.Value.OfType<object>().Select(value => (key: x.Key, value)))
                        .GroupBy(x => x.key, x => x.value);
                    OnThen?.Invoke(this, new FormDataInteraction(interaction, formStructure, formData));
                    return;
                }
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
        var incomingLength = formDataSource.SourceContentLength;

        if (!formStructure.AppliesTo(parsedMethod, candidateUrl, incomingContentType))
        {
            OnElse?.Invoke(this, interaction);
            return;
        }

        if (RequireContentLength && incomingLength == -1)
        {
            OnRejection?.Invoke(this, interaction);
            GlobalLog.Instance?.Debug(
                "No content length provided, but required for this form. Switch off requirecontentlength to be more lenient.");
            return;
        }

        RootByteReader limitedByteReader;
        if (incomingLength > -1)
        {
            if (incomingLength > CurrentByteLimit)
            {
                OnRejection?.Invoke(this, interaction);
                GlobalLog.Instance?.Debug("maxlength exceeded on Content Length header.");
                return;
            }
            else
            {
                limitedByteReader = new RootByteReader(formDataSource.SourceBuffer, incomingLength);
            }
        }
        else
        {
            limitedByteReader = new RootByteReader(formDataSource.SourceBuffer, CurrentByteLimit);
        }

        FormProgressInteraction progress = new(interaction, limitedByteReader, new());
        OnProgress?.Invoke(this, progress);
        try
        {
            if (incomingContentType == "application/x-www-form-urlencoded")
            {
                var urlEncodedReader = new UrlEncodedTokenReader(limitedByteReader);
                formData = new StreamingFormDataEnumerable(urlEncodedReader);
                OnThen?.Invoke(this, new FormDataInteraction(interaction, formStructure, formData));
            }
            else if (incomingContentType == "multipart/form-data")
            {
                if (!interaction.TryGetClosest<IContentTypePropertiesInteraction>(out var contTypeProperties)
                    || contTypeProperties == null
                    || !contTypeProperties.ContentTypeProperties.TryGetValue("boundary", out string? boundaryText))
                {
                    OnException?.Invoke(this,
                        new CommonInteraction(interaction,
                            "Multipart form data must have properties in the content type"));
                    return;
                }

                var prefixedByteReader = new PrefixedReader(limitedByteReader, "\r\n"u8.ToArray());
                var debugByteReader = prefixedByteReader;// new DebugReader(prefixedByteReader);
                var terminatingByteReader = MultibyteEotReader.CreateForAscii(
                    debugByteReader, $"--{boundaryText}--");
                var multipartEncodedReader = new MultipartGroupList(
                    terminatingByteReader, $"\r\n--{boundaryText}");
                OnThen?.Invoke(this, new FormDataInteraction(interaction, formStructure, multipartEncodedReader));
            }
            else
            {
                OnRejection?.Invoke(this, interaction);
            }
        }
        finally
        {
            progress.Finish();
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}