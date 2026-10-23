using SkiaSharp;
using Ziewaar.RAD.Doodads.CommonComponents.LiteralSource;
using Ziewaar.RAD.Doodads.CommonComponents.TextTemplating;
using Ziewaar.TtLog.Utilities;
using Ziewaar.Web.Uploadserver;
using Ziewaar.Web.Uploadserver.Extensions;
using Ziewaar.Web.Uploadserver.Interfaces;
using Ziewaar.Web.Uploadserver.Models;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.FastMultipart;

public class UploadForm : BasicService, IPlaceholderResolver, IProgressGrapher, IUploadValidator
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

    public override event CallForInteraction? OnThen;
    public override event CallForInteraction? OnElse;
    public event CallForInteraction? HaveLink;

    public UploadForm()
    {
        this.Router = new UploadRouter(
            this, this, new CommonUploadReceiver(
                this.UploadSize = new CommonUploadSizeGovernor(),
                new CommonMultipartUploadParserFactory(this)),
            this.UploadBoxes = new UploadBoxRepository());

        PrepareMultipartInterruptions();
    }

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
                if (Named(constants, "maxfilesize") is { } maxFileSizeCandidate)
                    this.UploadSize.HardLimit = maxFileSizeCandidate.ParseByteSize();
                if (Named(constants, "mindiskspace") is { } minDiskSpaceCandidate)
                    this.UploadSize.DiskSpaceLimit = minDiskSpaceCandidate.ParseByteSize();
                if (Named(constants, "maxfilecount") is { } maxFileCountCandidate &&
                    byte.TryParse(maxFileCountCandidate, out var maxFileCount))
                    this.MaxFileCount = maxFileCount;
                if (Named(constants, "barwidth") is { } barWidthCandidate &&
                    ushort.TryParse(barWidthCandidate, out var parsedBarWidth))
                    this.BarWidth = parsedBarWidth;
                if (Named(constants, "barheight") is { } barHeightCandidate &&
                    ushort.TryParse(barHeightCandidate, out var parsedBarHeight))
                    this.BarHeight = parsedBarHeight;

                var urlParts = NamedPartsFromMemory(constants, "action", interaction).ToArray();
                if (urlParts.Length == 0 || urlParts.Sum(x => x.Length) == 0)
                    throw new BasicException("absolute action url required");
                this.ActionUrl = string.Concat(urlParts);

                var extensionsAndMimes = NamedParts(constants, "accept").ToArray();
                if (extensionsAndMimes.Length == 0)
                    throw new BasicException("missing acceptable extensions and/or mime types in primary");
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
        FixedSkFont => field ??= new SKFont(SKTypeface.FromFile(FontPath, 0)) { Embolden = true };

    private static string
        FontPath => field ??= Directory.EnumerateFiles(SpecialPaths.AssemblyDirectory, "montserrat.ttf",
                                      new EnumerationOptions()
                                          { IgnoreInaccessible = true, RecurseSubdirectories = true })
                                  .FirstOrDefault() ??
                              throw new BasicException("font not found");

    private byte[] BareBoundaryBytes, CrlfCrlfBoundaryBytesCrlf, CrlfBytes, PartHeaderBytes;

    private void PrepareMultipartInterruptions()
    {
        this.BareBoundaryBytes = Encoding.ASCII.GetBytes("--PROGRESS");
        this.CrlfCrlfBoundaryBytesCrlf = Encoding.ASCII.GetBytes(
            "\r\n\r\n--PROGRESS\r\n");
        this.CrlfBytes = Encoding.ASCII.GetBytes("\r\n");
        this.PartHeaderBytes = Encoding.ASCII.GetBytes(
            "Content-Type: image/gif\r\nContent-Length: ");
    }

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

    private void UpdateProgressCanvas(SKCanvas cnv, long barSize, string message)
    {
        var center = new SKPoint((float)BarWidth / 2, (float)BarHeight / 2);
        cnv.Clear(SKColors.White);
        cnv.DrawRect(0, 0, barSize, BarHeight, BarPaint);

        var scenter = center;
        using var reader = new StringReader(message);
        while (reader.ReadLine() is string line)
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