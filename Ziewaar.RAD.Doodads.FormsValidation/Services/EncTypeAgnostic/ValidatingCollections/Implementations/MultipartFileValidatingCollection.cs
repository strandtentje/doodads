using System.Collections.Immutable;
using Ziewaar.RAD.Doodads.CommonComponents.LiteralSource;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Factories;

public class MultipartFileValidatingCollection(
    string expectedName,
    IImmutableSet<string> acceptedExtensions,
    IImmutableSet<string> acceptedMimes) : IValidatingCollection
{
    private readonly List<object> Transformations = new();
    public bool IsSatisfied { get; private set; } = true;
    public string Reason { get; private set; } = "";
    public IEnumerable ValidItems => Transformations;

    public void Add(object value, out object transformed)
    {
        transformed = value;
        if (!this.IsSatisfied) return;
        if (!MultipartHeaderValidation.Run(expectedName, value, out string headerFailureReason, out var data,
                out var disposition, out var contentType))
        {
            this.IsSatisfied = false;
            if (string.IsNullOrWhiteSpace(this.Reason)) this.Reason = headerFailureReason;
            return;
        }

        string? recoveredExtension = null;
        if (!disposition.HeaderArgs.TryGetValue("filename", out string? dirtyFilename))
        {
            MimeMapping.MimeTypeToExtension.TryGetValue(contentType.HeaderValue, out recoveredExtension);
            recoveredExtension ??= "bin";
            dirtyFilename = $"{Guid.NewGuid()}.{recoveredExtension}";
        }

        var cleanFilename = new string(
            dirtyFilename.Select(x => Path.GetInvalidFileNameChars().Contains(x) ? '_' : x).ToArray());
        var cleanExtension = $".{Path.GetExtension(cleanFilename)?.TrimStart('.') ?? "txt"}";
        if (!string.IsNullOrWhiteSpace(recoveredExtension))
            cleanFilename = cleanFilename[..^(recoveredExtension.Length + 1)] + cleanExtension;

        var mimeTypeFromFilename = MimeMapping.GetMimeInfo(cleanFilename);
        var mimeTypeFromHeader = contentType.HeaderValue;
        var headerAnnouncesText = mimeTypeFromFilename.IsText && mimeTypeFromHeader.StartsWith("text/");
        var formAcceptsText = acceptedMimes.Any(x => x.StartsWith("text/")) ||
                              acceptedExtensions.Contains(cleanExtension) ||
                              acceptedExtensions.Contains(".txt");

        if (headerAnnouncesText && formAcceptsText)
        {
            var detector = new MimeTypeDetector(data);
            if (!detector.IsText)
            {
                IsSatisfied = false;
                Reason = "Expected text file but received binary";
                return;
            }

            IsSatisfied = true;
            transformed = new TaggedReader(detector) { Tag = cleanFilename };
            Transformations.Add(transformed);
            return;
        }
        else if (mimeTypeFromFilename.MimeType == mimeTypeFromHeader &&
                 (acceptedMimes.Contains(mimeTypeFromHeader) || acceptedExtensions.Contains(cleanExtension)))
        {
            var detector = new MimeTypeDetector(data);
            if (detector.DetectedMime == mimeTypeFromHeader || acceptedMimes.Contains(detector.DetectedMime))
            {
                IsSatisfied = true;
                transformed = new TaggedReader(detector) { Tag = cleanFilename };
                Transformations.Add(transformed);
                return;
            }
        }
        else
        {
            IsSatisfied = false;
            Reason = "Mimetype Mismatch; rejected.";
            return;
        }
    }
}