using Ziewaar.RAD.Doodads.CommonComponents.LiteralSource;
using Ziewaar.Web.Uploadserver;
using Ziewaar.Web.Uploadserver.Interfaces;
using Ziewaar.Web.Uploadserver.Models;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.FastMultipart;

public class UploadForm : BasicService, ISessionSourcer, IPlaceholderResolver, IProgressGrapher, IUploadValidator
{
    private readonly CommonUploadSizeGovernor UploadSize;
    private readonly UploadBoxRepository UploadBoxes;
    private readonly UploadRouter Router;
    private readonly UploadBoxRepository Repository = new UploadBoxRepository();

    private readonly Lock AnchorLock = new();
    private string? AnchoredCookie;
    private string? AnchoredRoute;
    private byte MaxFileCount = 1;
    private HashSet<string>? AcceptableExtensions, AcceptableMimeTypes;

    private string AcceptString => field ??= string.Join(
        ',',
        (AcceptableExtensions ?? throw new BasicException("no acceptable extensions")).Concat
            (AcceptableMimeTypes ?? throw new BasicException("no acceptable mimes")));

    public override event CallForInteraction? OnThen;
    public override event CallForInteraction? OnElse;

    public UploadForm()
    {
        this.Router = new UploadRouter(
            this, this, this, new CommonUploadReceiver(
                this.UploadSize = new CommonUploadSizeGovernor(),
                new CommonMultipartUploadParserFactory(this)),
            this.UploadBoxes = new UploadBoxRepository());
    }

    public override void TryEnter(StampedMap constants, IInteraction interaction)
    {
        BasicException.ForInteraction(interaction, out IHttpIncomingRequestInteraction request);
        BasicException.ForInteraction(interaction, out ICookieInteraction currentCookie);
        BasicException.ForInteraction(interaction, out IRelativeRouteInteraction route);

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

                var extensionsAndMimes = PrimaryParts(constants).ToArray();
                if (extensionsAndMimes.Length == 0)
                    throw new BasicException("missing acceptable extensions and/or mime types in primary");
                var mimePairs = MimeMapping.ExpandMixedBag(extensionsAndMimes).ToArray();
                this.AcceptableExtensions = mimePairs.Select(x => x.extension).Distinct()
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
                this.AcceptableMimeTypes = mimePairs.Select(x => x.mimeType).Distinct()
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
            }

            if ((this.AnchoredCookie ??= currentCookie.CookieName) != currentCookie.CookieName)
                throw new BasicException("Uploadform anchored cookie changed");
            if ((this.AnchoredRoute ??= route.CurrentLocation) != route.CurrentLocation)
                throw new BasicException("Uploadform anchored route changed");
        }
    }

    string ISessionSourcer.GetSessionIdFor(HttpListenerRequest req)
    {
        string cookieName;
        lock (AnchorLock)
            cookieName = this.AnchoredCookie ?? throw new BasicException("Missing anchor cookie name");
        return req.Cookies[cookieName]?.Value ?? throw new BasicException("Missing cookie value");
    }

    bool IPlaceholderResolver.IsVirgin => false;

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

    void IProgressGrapher.Run(UploadBox selectedUploadBox, HttpListenerResponse rsp)
    {
        
    }

    bool IUploadValidator.TryValidate(string contentType, int fileCount) =>
        fileCount <= this.MaxFileCount &&
        (AcceptableMimeTypes ?? throw new BasicException("no acceptable mimetypes configured")).Contains(contentType);
}