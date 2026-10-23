using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Security.Cryptography;
using Ziewaar.RAD.Doodads.CommonComponents.LiteralSource;
using Ziewaar.RAD.Doodads.CommonComponents.TextTemplating;
using Ziewaar.Web.Uploadserver;
using Ziewaar.Web.Uploadserver.Interfaces;
using Ziewaar.Web.Uploadserver.Models;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.FastMultipart;

public class UploadFormHttpLifecycle(
    IHttpHeadInteraction head,
    ICookieInteraction cookie,
    IHttpIncomingRequestInteraction request,
    IHttpOutgoingResponseInteraction response,
    ISinkingInteraction sink,
    BufferSinkInteraction? plugBuffer) : IHttpLifecycleWrapper
{
    public string UpperHttpMethod => head.Method.ToUpper();
    public string SessionId => cookie.Register.ToString() ?? throw new BasicException("missing cookie");
    public Encoding? ResponseEncoding { get => response.TextEncoding; set { } }
    public Stream ResponseStream { get => response.SinkBuffer; }
    public Stream RequestStream { get => request.SourceBuffer; }
    public string? RequestContentType { get => request.SourceContentTypePattern; }
    public long RequestContentLength { get => request.SourceContentLength; }
    private IReadOnlyDictionary<string, string> QueryParameters => field ??=
        head.QueryString.
        Split('&', (StringSplitOptions)3).
        Select(assignment => (eqPos: assignment.IndexOf('='), assignment)).
        Where(x => x.eqPos > 0).
        Select(pair => (key: pair.assignment.Substring(0, pair.eqPos), value: pair.assignment.Substring(pair.eqPos + 1))).
        Select(pair => (key: HttpUtility.UrlDecode(pair.key), value: HttpUtility.UrlDecode(pair.value))).
        DistinctBy(x => x.key, StringComparer.OrdinalIgnoreCase).
        ToDictionary(x => x.key, x => x.value, StringComparer.OrdinalIgnoreCase);
    public void Bypass() => plugBuffer?.Bypass();
    public string GetFromQueryString(string key) => QueryParameters.TryGetValue(key, out var value) ? value : string.Empty;

    public void SetCacheControl(string value) => response.SetCacheControl(value);

    public void SetKeepAlive(bool ka) => response.SetKeepAlive(ka);

    public void TrySetResponseHeader(long contentLength, string contentType)
    {
        sink.SinkTrueContentType ??= contentType;        
    }    
}

public class UploadForm : BasicService, IPlaceholderResolver, IProgressGrapher, IUploadValidator
{
    private readonly CommonUploadSizeGovernor UploadSize;
    private readonly UploadBoxRepository UploadBoxes;
    private readonly UploadRouter Router;
    private readonly byte[] BareBoundaryBytes, CrlfCrlfBoundaryBytesCrlf, CrlfBytes, PartHeaderBytes;
    private readonly UploadBoxRepository Repository = new UploadBoxRepository();

    private readonly Lock AnchorLock = new();
    
    private string? AnchoredRoute;
    private byte MaxFileCount = 1;
    private HashSet<string>? AcceptableExtensions, AcceptableMimeTypes;
    private ushort BarWidth;
    private ushort BarHeight;

    private string AcceptString => field ??= string.Join(
        ',',
        (AcceptableExtensions ?? throw new BasicException("no acceptable extensions")).Concat
            (AcceptableMimeTypes ?? throw new BasicException("no acceptable mimes")));

    public override event CallForInteraction? OnThen;
    public override event CallForInteraction? OnElse;

    public UploadForm()
    {
        this.Router = new UploadRouter(
            this, this, new CommonUploadReceiver(
                this.UploadSize = new CommonUploadSizeGovernor(),
                new CommonMultipartUploadParserFactory(this)),
            this.UploadBoxes = new UploadBoxRepository());

        this.BareBoundaryBytes = Encoding.ASCII.GetBytes("--PROGRESS");
        this.CrlfCrlfBoundaryBytesCrlf = Encoding.ASCII.GetBytes(
            "\r\n\r\n--PROGRESS\r\n");
        this.CrlfBytes = Encoding.ASCII.GetBytes("\r\n");
        this.PartHeaderBytes = Encoding.ASCII.GetBytes(
            "Content-Type: image/png\r\nContent-Length: ");
    }

    public override void TryEnter(StampedMap constants, IInteraction interaction)
    {
        BasicException.ForInteraction(interaction, out IHttpIncomingRequestInteraction request);
        BasicException.ForInteraction(interaction, out IHttpOutgoingResponseInteraction response);
        BasicException.ForInteraction(interaction, out IHttpHeadInteraction head);
        BasicException.ForInteraction(interaction, out ICookieInteraction currentCookie);
        BasicException.ForInteraction(interaction, out ISinkingInteraction sink);
        BasicException.ForInteraction(interaction, out IRelativeRouteInteraction route);

        interaction.TryGetClosest<BufferSinkInteraction>(out var bsi);

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

                var extensionsAndMimes = PrimaryParts(constants).ToArray();
                if (extensionsAndMimes.Length == 0)
                    throw new BasicException("missing acceptable extensions and/or mime types in primary");
                var mimePairs = MimeMapping.ExpandMixedBag(extensionsAndMimes).ToArray();
                this.AcceptableExtensions = mimePairs.Select(x => x.extension).Distinct()
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
                this.AcceptableMimeTypes = mimePairs.Select(x => x.mimeType).Distinct()
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
            }

            if ((this.AnchoredRoute ??= route.CurrentLocation) != route.CurrentLocation)
                throw new BasicException("Uploadform anchored route changed");
        }

        Router.Route(new UploadFormHttpLifecycle(head, currentCookie, request, response, sink, bsi));
    }


    long IPlaceholderResolver.GetByteLengthFor(Encoding encoding, string p) => -1;

    void IPlaceholderResolver.WriteBytesFor(Encoding encoding, string p, Stream stream)
    {
        byte[] writeBytes = p switch
        {
            "url" => encoding.GetBytes(p),
            "multiple" when this.MaxFileCount > 1 => encoding.GetBytes("multiple"),
            "accept" => encoding.GetBytes(AcceptString),
            _ => Array.Empty<byte>()
        };

        if (writeBytes.Length > 0)
            stream.Write(writeBytes, 0, writeBytes.Length);
    }

    void IProgressGrapher.Run(UploadBox selectedUploadBox, IHttpLifecycleWrapper lifecycle)
    {        
        using var image = new Image<Rgba32>(BarWidth, BarHeight);
        lifecycle.Bypass();
        lifecycle.TrySetResponseHeader(-1, "multipart/x-mixed-replace; boundary=PROGRESS");
        lifecycle.SetCacheControl("no-cache");
        lifecycle.SetKeepAlive(true);

        lifecycle.ResponseStream.Write(BareBoundaryBytes, 0, BareBoundaryBytes.Length);
        lifecycle.ResponseStream.Write(CrlfBytes, 0, CrlfBytes.Length);
        
        while(selectedUploadBox.TryWaitForFinish(out var msg))
        {
            image.Mutate(x =>
            {
                
            });
        }
    }

    bool IUploadValidator.TryValidate(string contentType, int fileCount) =>
        fileCount <= this.MaxFileCount &&
        (AcceptableMimeTypes ?? throw new BasicException("no acceptable mimetypes configured")).Contains(contentType);
}