using Define.Doodads.Expo.Timeline;
using Ejije.Logging;
using Ziewaar.RAD.Doodads.CommonComponents.TextTemplating;
using Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.FormStructure;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic;

[Category("Input & Validation")]
[Title("Terse upload-only form")]
[Description("""
             This is a combination of FileTemplate/HtmlFormPrepare/Applicable/Print/Validate 
             for facilitating an upload form specifically. Provide path to main form html in
             primary constant, and stick upload progress html under OnUploading. 
             UploadForm service will then take care of iframes and all that jazz.
             """)]
public class UploadForm : BasicService
{
    [PrimarySetting("Path to main form template")]
    private readonly UpdatingPrimaryValue MainFormTemplate = new UpdatingPrimaryValue();
    [NamedSetting("interval", "Upload progress check interval in milliseconds")]
    private readonly UpdatingKeyValue ProgressInterval = new UpdatingKeyValue("interval");
    [NamedSetting("maxlength", "Max file size. Units allowed, ie. 25mb, 100kb")]
    private readonly UpdatingKeyValue MaxFileSize = new UpdatingKeyValue("maxlength");

    [EventOccasion("When the upload was done")]
    public override event CallForInteraction? OnThen;
    [EventOccasion("When form wasn't accepted")]
    public override event CallForInteraction? OnElse;
    [EventOccasion("Propagates any exception from the services that make uploadform work")]
    public override event CallForInteraction? OnException;
    [EventOccasion("When the incoming call wasn't applicable to our form")]
    public event CallForInteraction? OnRejection;
    [EventOccasion("""
        Sink upload form here; puts the following names in memory:        
        "position", "length", "rate",
        "progress", "size", "speed", 
        "percentage", "state"
        """)]
    public event CallForInteraction? OnUploading;

    private readonly StampedMap
        TemplateFileConstants = new(""), MaxLengthConstants = new(""), EmptyConstants = new("");

    private readonly EventWaitHandle ProgressWatchAbort = new EventWaitHandle(false, EventResetMode.ManualReset);
    private readonly Dictionary<string, List<IReadOnlyDictionary<string, object>>> UploadProgressMemories = new();

    private readonly HtmlFormPrepare FormPrepare = new HtmlFormPrepare();
    private readonly FileTemplate FileTemplate = new FileTemplate();
    private readonly HtmlFormApplicable FormApplicable = new HtmlFormApplicable();
    private readonly HtmlFormPrint FormPrint = new HtmlFormPrint();
    private readonly HtmlFormValidate FormValidate = new HtmlFormValidate();

    private const string PROGRESS_IFRAME = "progressiframe", FORM_IFRAME = "formiframe";

    private int Interval = 1;
    private static readonly Log
        NotApplicable = Log.Tech("Form {fmethod} {action} was not applicable for validation on {rmethod} {url}"),
        DisplayProgress = Log.Tech("For form {fmethod} {action}, displaying progress window"),
        DisplayEntry = Log.Tech("For form {fmethod} {action}, displaying entry window"),
        DisplayIframes = Log.Tech("For form {fmethod} {action}, displaying iframes"),
        Uploading = Log.Cool("File upload {x}b of {y}b");
    public UploadForm()
    {
        FormPrepare.OnException += (s, e) => OnException?.Invoke(s, e);
        FileTemplate.OnException += (s, e) => OnException?.Invoke(s, e);
        FormApplicable.OnException += (s, e) => OnException?.Invoke(s, e);
        FormPrint.OnException += (s, e) => OnException?.Invoke(s, e);
        FormValidate.OnException += (s, e) => OnException?.Invoke(s, e);

        FormPrepare.OnElse += (s, e) =>
        {
            FileTemplate.Enter(TemplateFileConstants, e);
        };
        FormPrepare.OnThen += (s, e) =>
        {
            FormApplicable.Enter(MaxLengthConstants, e);
        };
        FormApplicable.OnElse += (s, e) =>
        {
            var httpHead = e.TryGetClosest<IHttpHeadInteraction>(out var head) ? head : null;
            if (httpHead == null)
                throw new BasicException("Http required");
            if (!e.TryGetClosest<FormStructureInteraction>(out var fsi) || fsi == null)
                throw new BasicException("Form structure missing");
            if (!e.TryGetClosest<ISinkingInteraction>(out var cpi) || cpi == null)
                throw new BasicException("Sink required");

            Log.Post(NotApplicable,
                fsi.HttpMethod, fsi.ActionUrl, httpHead.Method, httpHead.RouteString);

            var progressRequested = httpHead.QueryString.Contains(PROGRESS_IFRAME);
            var formRequested = httpHead.QueryString.Contains(FORM_IFRAME);

            if (progressRequested)
            {
                Log.Post(DisplayProgress, fsi.HttpMethod, fsi.ActionUrl);

                if (e.TryGetClosest<BufferSinkInteraction>(out var bsi) && bsi != null)
                {
                    bsi.Bypass();
                    cpi.SinkTrueContentType = "text/html";
                }

                cpi.Write($"""
                    <!DOCTYPE html>
                    <html>
                    <head>
                    	<meta charset="utf-8">
                    	<meta name="viewport" content="width=device-width, initial-scale=1">
                        <meta http-equiv="refresh" content="{Interval}">
                    </head>
                    <body>
                    """);

                var cookie = e.TryGetClosest<ICookieInteraction>(out var val) ? val : null;
                var cookieValue = cookie?.Register?.ToString();
                if (cookieValue == null)
                    throw new BasicException("Cookie required");
                if (UploadProgressMemories.TryGetValue(cookieValue, out var progresses))
                {
                    foreach (var item in progresses)
                    {
                        OnUploading?.Invoke(this, e.AppendMemory(item));
                    }
                }

                cpi.Write("""</body></html>""");

                if (e.TryGetClosest<BufferSinkInteraction>(out var bsi2) && bsi2 != null)
                {
                    bsi2.Terminate();
                }
            }
            else if (formRequested)
            {
                if (e.TryGetClosest<BufferSinkInteraction>(out var bsi) && bsi != null)
                {
                    bsi.Bypass();
                    cpi.SinkTrueContentType = "text/html";
                }

                Log.Post(DisplayEntry,
                    fsi.HttpMethod, fsi.ActionUrl);

                cpi.Write("""
                    <!DOCTYPE html>
                    <html>
                    <head>
                    	<meta charset="utf-8">
                    	<meta name="viewport" content="width=device-width, initial-scale=1">
                    </head>
                    <body>
                    """);
                FormPrint.Enter(EmptyConstants, e);
                cpi.Write("""</body></html>""");
                if (e.TryGetClosest<BufferSinkInteraction>(out var bsi2) && bsi2 != null)
                {
                    bsi2.Terminate();
                }
            }
            else
            {
                string progressUrl, formUrl;

                Log.Post(DisplayIframes, fsi.HttpMethod, fsi.ActionUrl);

                var parts = fsi.ActionUrl.Split('?');
                if (parts.Length == 1)
                {
                    progressUrl = $"{parts[0]}?{PROGRESS_IFRAME}";
                    formUrl = $"{parts[0]}?{FORM_IFRAME}";
                }
                else
                {
                    progressUrl = $"{parts[0]}?{parts[1]}&{PROGRESS_IFRAME}";
                    formUrl = $"{parts[0]}?{parts[1]}&{FORM_IFRAME}";
                }

                cpi.Write($"""
                    <iframe class="formframe" src="{formUrl}"></iframe>
                    <iframe class="progressframe" src="{progressUrl}"></iframe>
                    """);
            }
        };
        FormApplicable.OnThen += (s, e) =>
        {
            FormValidate.Enter(EmptyConstants, e);
        };
        FormApplicable.OnRejection += (s, e) =>
        {
            this.OnRejection?.Invoke(this, e);
        };
        FormApplicable.OnProgress += (s, e) =>
        {
            var formProgress = e.TryGetClosest<FormProgressInteraction>(out var fpi) ? fpi : null;
            if (formProgress == null)
                throw new BasicException("Form progress required");

            var formProgressMemory = formProgress.Memory;

            var cookie = e.TryGetClosest<ICookieInteraction>(out var val) ? val : null;
            var cookieValue = cookie?.Register?.ToString();
            if (cookieValue == null)
                throw new BasicException("Cookie required");

            ThreadPool.QueueUserWorkItem(_ =>
            {
                if (!UploadProgressMemories.TryGetValue(cookieValue, out var progressMemoryList))
                    UploadProgressMemories[cookieValue] = progressMemoryList = new();

                progressMemoryList.Add(formProgressMemory);

                try
                {
                    while (!ProgressWatchAbort.WaitOne(500) &&
                        formProgress.Reader.ErrorState == null &&
                        formProgress.Reader.AtEnd == false)
                    {
                        Log.Post(Uploading, formProgress.Reader.Cursor, formProgress.Reader.Limit);
                    }
                }
                catch (ObjectDisposedException)
                {
                    // eh
                }
                finally
                {
                    progressMemoryList.Remove(formProgressMemory);
                }
            }, null);
        };
        FormValidate.OnThen += (s, e) =>
        {
            if (!e.TryGetClosest<ISinkingInteraction>(out var cpi) || cpi == null)
                throw new BasicException("Sink required");
            if (e.TryGetClosest<BufferSinkInteraction>(out var bsi) && bsi != null)
            {
                bsi.Bypass();
                cpi.SinkTrueContentType = "text/html";
            }
            OnThen?.Invoke(this, e);

            if (e.TryGetClosest<BufferSinkInteraction>(out var bsi2) && bsi2 != null)
            {
                bsi2.Terminate();
            }
        };
        FormValidate.OnElse += (s, e) =>
        {
            if (!e.TryGetClosest<ISinkingInteraction>(out var cpi) || cpi == null)
                throw new BasicException("Sink required");
            if (e.TryGetClosest<BufferSinkInteraction>(out var bsi) && bsi != null)
            {
                bsi.Bypass();
                cpi.SinkTrueContentType = "text/html";
            }
            OnElse?.Invoke(this, e);

            if (e.TryGetClosest<BufferSinkInteraction>(out var bsi2) && bsi2 != null)
            {
                bsi2.Terminate();
            }
        };
    }

    public override void TryEnter(StampedMap constants, IInteraction interaction)
    {
        if (constants.NamedItems.TryGetValue("interval", out var ival) && ival != null)
        {
            try
            {
                Interval = Convert.ToInt32(ival);
            }
            catch (Exception)
            {
                throw new BasicException("Invalid interval number");
            }
        }
        TemplateFileConstants.SetPrimary(constants.PrimaryConstant);
        MaxLengthConstants.SetValue("maxlength", constants.NamedItems.TryGetValue("maxlength", out var val) ? val : "4gb");
        FormPrepare.Enter(EmptyConstants, interaction);
    }
}