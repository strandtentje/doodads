using Ziewaar.RAD.Doodads.EnumerableStreaming.Readers;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Factories;

public class MultipartParameterValidatingCollection(
    string expectedName) : IValidatingCollection
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

        bool isGivenPlainText = "text/plain".Equals(contentType?.HeaderValue, StringComparison.OrdinalIgnoreCase);
        var isGivenFilename = disposition?.HeaderArgs.TryGetValue("filename", out string? dirtyFilename) == true;

        string? charset = null;
        contentType?.HeaderArgs.TryGetValue("charset", out charset);
        charset ??= "utf-8";

        if (!"utf-8".Equals(charset, StringComparison.OrdinalIgnoreCase))
        {
            IsSatisfied = false;
            Reason = "Charset must be utf8 for multipart forms.";
            return;
        }

        if (!isGivenPlainText || isGivenFilename)
        {
            IsSatisfied = false;
            Reason = $"Was given file or non-textual data, but not expecting one on {expectedName}";
        }
        else
        {
            var unicodeReader = new UnicodeConvertingReader(data);
            IsSatisfied = true;
            transformed = unicodeReader;
            Transformations.Add(transformed);
        }
    }
}