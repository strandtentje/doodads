using SkiaSharp;
using Ziewaar.RAD.Doodads.CommonComponents.LiteralSource;
using Ziewaar.RAD.Doodads.CommonComponents.TextTemplating;
using TtLog;
using TtLog.Utilities;
using Ziewaar.Web.Uploadserver;
using Ziewaar.Web.Uploadserver.Extensions;
using Ziewaar.Web.Uploadserver.Interfaces;
using Ziewaar.Web.Uploadserver.Models;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.FastMultipart;

[Category("Input & Validation")]
[Title("A fully integrated upload form")]
[Description("""
             Put a target directory in register before calling this service, or your files are gonna end up 
             all sorts of weird places.
             
             Fully generates an upload form and conducts its lifecycle. This service requires a Session and a SinkPlug
             to have preceeded it. Uploads are not accepted willy-nilly, instead a transient upload session is generated
             for each upload job. In case this Service finds the current upload session incomplete or unsuitable,
             it'll go into HaveLink; you will need to ie. pass this link to the user. 
             
             Uploads that did not succeed trigger OnElse, uploads that did succeed trigger OnThen; which may then be 
             used with UploadedFileList to get a list of the files that have been uploaded. 
             
             Upon GET and a matching upload transient, this service generates the <form>-html automatically, as opposed
             to HtmlForm which requires you to provide a <form>-html with validation rules. This html is a form with 
             only a file field, configured to match the way this service has been configured. Thise html also contains
             an <img> tag that calls the PROGRESS-function on this service; this provides a multipart/x-mixed-replace 
             stream of webp's that show the upload progress. 
             
             An upload transient will always get purged and disposed after use, regardless of whether or not the 
             upload succeeded. If part of the upload was OK, but another part wasn't, no files related to the upload
             will be persisted. 
             """)]
public class UploadForm : BasicService, IPlaceholderResolver, IProgressGrapher, IUploadValidator,
    ITransferRateConstraints
{
    private readonly CommonUploadSizeGovernor UploadSize;
    private readonly UploadBoxRepository UploadBoxes;
    private readonly UploadRouter Router;

    private readonly Lock AnchorLock = new();

    private byte MaxFileCount = 1;
    private HashSet<string>? AcceptableExtensions, AcceptableMimeTypes;
    private ushort BarWidth;
    private ushort BarHeight;
    private string? ActionUrl;

    private string AcceptString => field ??= string.Join(
        ',',
        (AcceptableExtensions ?? throw new BasicException("no acceptable extensions")).Concat
            (AcceptableMimeTypes ?? throw new BasicException("no acceptable mimes")));

    [EventOccasion("""
                   when the upload completed successfully, you may use a UploadedFileList here to iterate through
                    uploaded files
                   """)]
    public override event CallForInteraction? OnThen;
    [EventOccasion("when the upload failed, a message may be in register. the failure mode is also in the logs")]
    public override event CallForInteraction? OnElse;
    [EventOccasion("make sure to use this branch to provide a suitable transient upload link to the user.")]
    public event CallForInteraction? HaveLink;

    public UploadForm()
    {
        this.Router = new UploadRouter(
            this, this, new CommonUploadReceiver(
                this.UploadSize = new CommonUploadSizeGovernor(), this, this),
            this.UploadBoxes = new UploadBoxRepository());

        SetTransferRate10M();
    }

    [NamedSetting("maxfilesize", "limit of upload size in bytes, or suffix number with kb, mb, gb, etc.")]
    private readonly UpdatingKeyValue MaxFileSizeConstant = new UpdatingKeyValue("maxfilesize");
    [NamedSetting("mindiskspace", "amount of disk space to preserve in bytes, or suffix number with mb, gb, etc.")]
    private readonly UpdatingKeyValue MinDiskSpaceConstant = new UpdatingKeyValue("mindiskspace");
    [NamedSetting("maxfilecount", "max. number of files to allow in upload.")]
    private readonly UpdatingKeyValue MaxFileCountConstant = new UpdatingKeyValue("maxfilecount");
    [NamedSetting("barwidth", "width of progress bar in pixels")]
    private readonly UpdatingKeyValue BarWidthConstant = new UpdatingKeyValue("barwidth");
    [NamedSetting("barheight", "height  of progress bar in pixels")]
    private readonly UpdatingKeyValue BarHeightConstant = new UpdatingKeyValue("barheight");
    [NamedSetting("rate", "upload rate limit; 1 for ~1mbps, 10 for ~10mbps and super for unlimited (very fast!)")]
    private readonly UpdatingKeyValue RateLimitConstant = new UpdatingKeyValue("rate");
    [NamedSetting("action", "full absolute working url of the form")]
    private readonly UpdatingKeyValue ActionConstant = new UpdatingKeyValue("action");
    [NamedSetting("accept", """
                            array of acceptable extensions and mime types. use `.extensions` and `mime/types` separately 
                            to have it automatically make a mime-extension combo, or supply `.extension=mime/type` 
                            for custom situations
                            """)]
    private readonly UpdatingKeyValue AcceptConstant = new UpdatingKeyValue("accept");
    
    public override void TryEnter(StampedMap constants, IInteraction interaction)
    {
        BasicException.ForInteraction(interaction, out IHttpIncomingRequestInteraction request);
        BasicException.ForInteraction(interaction, out IHttpOutgoingResponseInteraction response);
        BasicException.ForInteraction(interaction, out IHttpHeadInteraction head);
        BasicException.ForInteraction(interaction, out ICookieInteraction currentCookie);
        BasicException.ForInteraction(interaction, out ISinkingInteraction sink);
        BasicException.ForInteraction(interaction, out BufferSinkInteraction bsi);

        lock (AnchorLock)
        {
            if (this.AcceptableMimeTypes == null || this.AcceptableExtensions == null)
            {
                if (Named(constants, MaxFileSizeConstant) is { } maxFileSizeCandidate)
                    this.UploadSize.HardLimit = maxFileSizeCandidate.ParseByteSize();
                if (Named(constants, MinDiskSpaceConstant) is { } minDiskSpaceCandidate)
                    this.UploadSize.DiskSpaceLimit = minDiskSpaceCandidate.ParseByteSize();
                if (Named(constants, MaxFileCountConstant) is { } maxFileCountCandidate &&
                    byte.TryParse(maxFileCountCandidate, out var maxFileCount))
                    this.MaxFileCount = maxFileCount;
                if (Named(constants, BarWidthConstant) is { } barWidthCandidate &&
                    ushort.TryParse(barWidthCandidate, out var parsedBarWidth))
                    this.BarWidth = parsedBarWidth;
                if (Named(constants, BarHeightConstant) is { } barHeightCandidate &&
                    ushort.TryParse(barHeightCandidate, out var parsedBarHeight))
                    this.BarHeight = parsedBarHeight;

                if (Named(constants, RateLimitConstant) is { } rateCandidate)
                {
                    switch (rateCandidate.ToLower().Trim())
                    {
                        case "1":
                            SetTransferRate1M();
                            break;
                        case "super":
                            SetTransferRateSuper();
                            break;
                        // case "10":
                        default:
                            SetTransferRate10M();
                            break;
                    }
                }

                var urlParts = NamedPartsFromMemory(constants, ActionConstant, interaction).ToArray();
                if (urlParts.Length == 0 || urlParts.Sum(x => x.Length) == 0)
                    throw new BasicException("absolute action url required");
                this.ActionUrl = string.Concat(urlParts);

                var extensionsAndMimes = NamedParts(constants, AcceptConstant).ToArray();
                if (extensionsAndMimes.Length == 0)
                    throw new BasicException("missing acceptable extensions and/or mime types in accept");
                var mimePairs = MimeMapping.ExpandMixedBag(extensionsAndMimes).ToArray();
                this.AcceptableExtensions = mimePairs.Select(x => x.extension).Distinct()
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
                this.AcceptableMimeTypes = mimePairs.Select(x => x.mimeType).Distinct()
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
            }
        }

        IEnumerable<string> receivedFiles = Enumerable.Empty<string>();
        var routingResult =
            StringComparer.Ordinal.Equals(this.ActionUrl, head.RouteString)
                ? Router.Route(new UploadFormHttpLifecycle(head, currentCookie, request, response, sink, bsi),
                    out receivedFiles)
                : UploadRoutingState.MakeBox;

        if (routingResult.HasFlag(UploadRoutingState.MakeBox))
        {
            var cookie = currentCookie.Register.ToString();
            if (string.IsNullOrWhiteSpace(cookie))
                throw new BasicException("empty cookie not allowed");
            var suitableUploadBox = UploadBoxes.Enter(cookie, DirectoryFromRegister(interaction), ["files"]);
            HaveLink?.Invoke(this, interaction.AppendRegister(
                $"{this.ActionUrl}?uploadBoxId={suitableUploadBox.UploadBoxId}&amp;functionId=PAGE"));
        }
        else if (routingResult == UploadRoutingState.UploadDone)
            OnThen?.Invoke(this, interaction.AppendCustom(new FinishedUpload(receivedFiles)));
        else if (routingResult.HasFlag(UploadRoutingState.Failure))
            OnElse?.Invoke(this, interaction.AppendRegister("upload failed"));
    }

    long IPlaceholderResolver.GetByteLengthFor(Encoding encoding, string p) => -1;

    void IPlaceholderResolver.WriteBytesFor(Encoding encoding, string p, Stream stream)
    {
        byte[] writeBytes = p switch
        {
            "url" => encoding.GetBytes(this.ActionUrl ?? throw new BasicException("missing action url")),
            "multiple" when this.MaxFileCount > 1 => encoding.GetBytes("multiple"),
            "accept" => encoding.GetBytes(AcceptString),
            _ => Array.Empty<byte>()
        };

        if (writeBytes.Length > 0)
            stream.Write(writeBytes, 0, writeBytes.Length);
    }

    private static readonly SKPaint
        FontPaint = new SKPaint()
        {
            Color = SKColors.Black, Style = SKPaintStyle.Fill, IsAntialias = true,
        },
        BarPaint = new SKPaint()
        {
            Color = SKColors.DodgerBlue, Style = SKPaintStyle.Fill, IsAntialias = true
        };

    private static SKFont
        FixedSkFont => field ??= new SKFont(SKTypeface.FromFile(FontPath)) { Embolden = true };

    private static string
        FontPath => field ??= Directory.EnumerateFiles(SpecialPaths.AssemblyDirectory, "montserrat.ttf",
                                      new EnumerationOptions()
                                          { IgnoreInaccessible = true, RecurseSubdirectories = true })
                                  .FirstOrDefault() ??
                              throw new BasicException("font not found");

    private void SetTransferRate1M()
    {
        this.Sleep = 10;
        this.Burst = 1;
        this.BufferSize = 12 * 1024;
    }

    private void SetTransferRate10M()
    {
        this.Sleep = 3;
        this.Burst = 2;
        this.BufferSize = 24 * 1024;
    }

    private void SetTransferRateSuper()
    {
        this.Sleep = -1;
        this.Burst = 0;
        this.BufferSize = 16 * 1024;
    }

    public int Sleep { get; private set; } = 1;
    public int BufferSize { get; private set; } = 16 * 1024;
    public int Burst { get; private set; } = 1;

    private byte[] BareBoundaryBytes => field ??= [.. "--PROGRESS"u8];
    private byte[] CrlfCrlfBoundaryBytesCrlf => field ??= [.. "\r\n\r\n--PROGRESS\r\n"u8];
    private byte[] CrlfBytes => field ??= [.. "\r\n"u8];
    private byte[] PartHeaderBytes => field ??= [.. "Content-Type: image/gif\r\nContent-Length: "u8]; 

    private static readonly Log ProgressStopped = Log.Oops("Progress bar {stopped}; window likely closed.");
    
    void IProgressGrapher.Run(UploadBox selectedUploadBox, IHttpLifecycleWrapper lifecycle)
    {
        var metaImage = new SKImageInfo(BarWidth, BarHeight, SKColorType.Rgba8888, SKAlphaType.Premul,
            SKColorSpace.CreateSrgb());

        using var responseStream = lifecycle.ResponseStream;
        using var surface = SKSurface.Create(metaImage);
        using var cnv = surface.Canvas;

        lifecycle.Bypass();
        lifecycle.TrySetResponseHeader(-1, "multipart/x-mixed-replace; boundary=PROGRESS");
        lifecycle.SetCacheControl("no-cache");
        lifecycle.SetKeepAlive(true);

        responseStream.Write(BareBoundaryBytes, 0, BareBoundaryBytes.Length);
        responseStream.Write(CrlfBytes, 0, CrlfBytes.Length);

        try
        {
            string message;
            while (!selectedUploadBox.TryWaitForFinish(out message))
            {
                message = selectedUploadBox.ContentLengthAnnounced > 0
                    ? string.Format("{0}%, {1}/s, {2}", selectedUploadBox.Percentage,
                        selectedUploadBox.GetNormalizedSpeed(), selectedUploadBox.GetNormalizedSize())
                    : "Nothing Currently Uploading";
                var barSize = (selectedUploadBox.Percentage * BarWidth) / 100;

                UpdateProgressCanvas(cnv, barSize, message);
                FlushProgressSurface(surface, responseStream);
            }

            UpdateProgressCanvas(cnv, BarWidth, message);
            FlushProgressSurface(surface, responseStream);
        }
        catch (Exception ex)
        {
            Log.Post(ProgressStopped, ex);
        }
    }

    private void UpdateProgressCanvas(SKCanvas cnv, long barSize, string message)
    {
        var center = new SKPoint((float)BarWidth / 2, (float)BarHeight / 2);
        cnv.Clear(SKColors.White);
        cnv.DrawRect(0, 0, barSize, BarHeight, BarPaint);

        var scenter = center;
        using var reader = new StringReader(message);
        while (reader.ReadLine() is { } line)
        {
            cnv.DrawText(line, scenter, SKTextAlign.Center, FixedSkFont, FontPaint);
            scenter.Y += 16;
        }
    }

    private void FlushProgressSurface(SKSurface surface, Stream responseStream)
    {
        using var snsh = surface.Snapshot();
        using var enc = snsh.Encode(SKEncodedImageFormat.Webp, 100);

        responseStream.Write(PartHeaderBytes, 0, PartHeaderBytes.Length);
        var contentLengthBytes = Encoding.ASCII.GetBytes(enc.Size.ToString());
        responseStream.Write(contentLengthBytes, 0, contentLengthBytes.Length);
        responseStream.Write(CrlfBytes, 0, CrlfBytes.Length);
        responseStream.Write(CrlfBytes, 0, CrlfBytes.Length);
        enc.SaveTo(responseStream);
        responseStream.Write(CrlfCrlfBoundaryBytesCrlf, 0, CrlfCrlfBoundaryBytesCrlf.Length);
    }


    bool IUploadValidator.TryValidate(string contentType, int fileCount) =>
        fileCount <= this.MaxFileCount &&
        (AcceptableMimeTypes ?? throw new BasicException("no acceptable mimetypes configured")).Contains(contentType);
}