using Ejije.Logging;
using Ziewaar.RAD.Doodads.FormsValidation.Services.Support;
using Ziewaar.Web.Uploadserver;
using Ziewaar.Web.Uploadserver.Models;

#pragma warning disable 67

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.UrlEncodedOnly;

public record UploadDirectoryAssignment(string directory, long preserveDiskSpace);

[Category("Input & Validation")]
[Title("Set upload directory on html form")]
[Description("In register or primary constant, provide the upload directory that succeeding htmlforms should use.")]
public class UploadDirectory : BasicService
{
    public override event CallForInteraction? OnThen;
    private static readonly UploadBoxRepository UboxRepo = new UploadBoxRepository();
    public override void TryEnter(StampedMap constants, IInteraction interaction)
    {
        BasicException.ForInteraction(interaction, out ICsrfTokenSourceInteraction csrfToken);
        
        var directory = this.DirectoryFromPrimaryOrRegister(constants, interaction);
        
        UboxRepo.Upsert(csrfToken.RawCsrfCookie, directory, 

        var freeDiskString = this.Named(constants, "freedisk");
        
        if (string.IsNullOrWhiteSpace(freeDiskString) || !freeDiskString.TryParseByteSize(out var remainingDiskSpace))
            remainingDiskSpace = 8L * 1024L * 1024L * 1024L;
        
        OnThen?.Invoke(this, interaction.AppendCustom(new UploadDirectoryAssignment(directory, remainingDiskSpace)));
    }
}

[Category("Input & Validation")]
[Title("Parse and Validate HTML form")]
[Description("""
             Provided a standard HTML form, this service 
              - Figures out to which route its data is posted
              - With what method its posted (POST->Body, GET->Query) 
              - In case the method and route apply, validates.
              - Optionally replaces field names with CSRF hardened field names
             It will always print out the form to the nearest sink unless: 
              - There was no method or maybe a not a query in memory
              - OnValid or OnInvalid triggered a redirect right after validation but before printing
             """)]
[ShortNames("hfrm")]
public class HtmlForm : IService
{
    private readonly UpdatingKeyValue MaxPostSize = new UpdatingKeyValue("maxsize");

    [EventOccasion("Sink the form text here.")]
    public event CallForInteraction? OnThen;

    [NeverHappens] public event CallForInteraction? OnElse;

    [EventOccasion("When the form was valid, the form values are in memory here.")]
    public event CallForInteraction? OnValid;

    [EventOccasion("When the form was invalid, no new data is here, but the form will be printed with its error spans")]
    public event CallForInteraction? OnInvalid;

    [EventOccasion("When there was no sink")]
    public event CallForInteraction? OnException;

    private static readonly Log
        FormError = Log.Oops("Form {method} {url} didn't come in quite right"),
        FieldError = Log.Tech("On item {name} we got a failure:{result} due to value {value}");

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (!interaction.TryGetClosest<ISinkingInteraction>(out var targetSink) || targetSink == null)
        {
            OnException?.Invoke(this,
                new CommonInteraction(interaction, "may only do this where there's another sink"));
            return;
        }

        var tsi = new TextSinkingInteraction(interaction, textEncoding: targetSink.TextEncoding);
        OnThen?.Invoke(this, tsi);
        var x = tsi.ReadAllText();
        HtmlDocument doc = new();
        doc.LoadHtml(x);
        var fieldset = ValidatingInputFieldSet.Parse(doc);

        ICsrfFields? csrfFields = interaction.TryGetClosest<ICsrfTokenSourceInteraction>(out var csrf) && csrf != null
            ? csrf.Fields
            : null;

        if (!constants.NamedItems.TryGetValue("maxsize", out var maxSizeStringCandidate) ||
            maxSizeStringCandidate is not string maxSizeString || string.IsNullOrWhiteSpace(maxSizeString) ||
            !maxSizeString.TryParseByteSize(out var maxContentLength))
            maxContentLength = 4 * 1024 * 1024;
        
        if (interaction.TryFindVariable<string>("method", out string? candidateMethod) && candidateMethod != null &&
            interaction.TryFindVariable<string>("url", out string? candiateUrl) && candiateUrl != null &&
            fieldset.IsValidationRequired(candidateMethod, candiateUrl))
        {
            IReadOnlyDictionary<string, IEnumerable>? parsedForm = null;
            if (fieldset.Method == HttpMethod.Post
                && interaction.TryGetClosest<ISourcingInteraction>(out var bodySource)
                && bodySource != null
                && fieldset.IsUrlEncodedAndMatches(bodySource.SourceContentTypePattern))
            {
                if (maxContentLength > 16 * 1024 * 1024)
                {
                    OnException?.Invoke(this,
                        interaction.AppendRegister("max size of regular url encoded form may not exceed 16mb"));
                    return;
                }
                else if (bodySource.SourceContentLength > maxContentLength)
                {
                    OnInvalid?.Invoke(this, interaction.AppendRegister("too big a form announced"));
                    return;
                }

                byte[] fullPostBody = new byte[(int)bodySource.SourceContentLength];
                byte[] partPostBody = new byte[4096];
                int dataRead = 0;

                for (var partRead = bodySource.SourceBuffer.Read(partPostBody, 0, partPostBody.Length);
                     (dataRead + partRead) <= bodySource.SourceContentLength && partRead != 0;
                     partRead = bodySource.SourceBuffer.Read(partPostBody, 0, partPostBody.Length))
                {
                    Array.Copy(partPostBody, 0, fullPostBody, dataRead, partRead);
                    dataRead += partRead;
                }

                if (dataRead != bodySource.SourceContentLength)
                {
                    OnInvalid?.Invoke(this, interaction.AppendRegister("content-length not satisfied"));
                    return;
                }

                string formText = bodySource.TextEncoding.GetString(fullPostBody);
                parsedForm = new FormDataDictionary(formText);
            }
            else if (fieldset.Method == HttpMethod.Get
                     && interaction.TryFindVariable<string>("query", out string? queryString) && queryString != null
                     && fieldset.IsUrlEncodedAndMatches(ValidatingInputFieldSet.URL_ENCODED_CONTENT_TYPE))
            {
                parsedForm = new FormDataDictionary(queryString);
            }
            else if (fieldset.Method == HttpMethod.Post
                     && interaction.TryGetClosest<ISourcingInteraction>(out var multipartSource)
                     && multipartSource is IHttpIncomingRequestInteraction requestInteraction
                     && fieldset.IsMultipartAndMatches(multipartSource.SourceContentTypePattern))
            {
                if (!interaction.TryGetCustom<UploadDirectoryAssignment>(out var target) || target == null)
                {
                    OnException?.Invoke(this,
                        interaction.AppendRegister("file upload form must be preceeded with uploaddirectory service."));
                    return;
                }

                var recv = new CommonUploadReceiver(
                    new CommonUploadSizeGovernor(maxContentLength, target.preserveDiskSpace),
                    new CommonMultipartUploadParserFactory(fieldset));
                var upbox = UploadBox.CreateOrphan(target.directory, [.. fieldset.FieldNames]);
                if (!recv.TryValidateUpload(requestInteraction.FullContentTypeHeader,
                        requestInteraction.SourceContentLength,
                        upbox, out var preValidationState))
                {
                    OnInvalid?.Invoke(this, interaction.AppendRegister($"upload form start fail {preValidationState}"));
                    return;
                }
                
                recv.TryReceiveUpload(multipartSource.SourceBuffer, upbox)

            }
            else
            {
                OnException?.Invoke(this, interaction.AppendRegister("bad method or no data"));
                return;
            }

            if (csrfFields != null)
                parsedForm = new DeobfuscatedFormDictionary(parsedForm, csrfFields, fieldset.ToString());
            try
            {
                var result = fieldset.Validate(parsedForm);
                if (!result.Any(x => x.isError))
                {
                    SortedList<string, object> saneValues = new();
                    foreach (var item in result)
                    {
                        if (item.value is IEnumerable enumerable)
                        {
                            var instances = enumerable.OfType<object>().ToArray();
                            if (instances.Length == 1)
                            {
                                saneValues[item.name] = instances[0];
                            }
                            else if (instances.Length > 1)
                            {
                                saneValues[item.name] = instances;
                            }
                        }
                        else if (item.value is object singleItem)
                        {
                            saneValues[item.name] = singleItem;
                        }
                    }

                    OnValid?.Invoke(this, new CommonInteraction(interaction, saneValues));
                }
                else
                {
                    Log.Post(FormError, candidateMethod,
                        candiateUrl);
                    foreach (var item in result)
                    {
                        Log.Post(FieldError, item.name, item.isError, item.value);
                    }

                    foreach (var item in result.Where(x => x.isError))
                    {
                        doc.SetErrorSpan(item.name);
                    }

                    OnInvalid?.Invoke(this, interaction);
                }
            }
            finally
            {
                if (parsedForm is IDisposable disposable) disposable.Dispose();
            }
        }

        if (csrfFields != null)
            fieldset.ApplyObfuscation(csrfFields);

        if (interaction.TryGetClosest<IMayRedirectInteraction>(out var redirect) &&
            redirect is { IsRedirecting: true }) return;

        // targetSink.SinkBuffer.Write(Encoding.UTF8.GetBytes("START FORM WRITE"));
        doc.Save(targetSink.SinkBuffer, targetSink.TextEncoding);
        // targetSink.SinkBuffer.Write(Encoding.UTF8.GetBytes("END FORM WRITE"));
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}