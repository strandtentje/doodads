namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic;
public class MultipartValueGroup : IGrouping<string, object>
{
    private readonly MultibyteEotReader CrlfDetector;
    private readonly UntilBoundaryReader BoundaryDetector;
    public MultipartValueGroup? NextGroup { get; private set; }
    private MultipartHeader[]? Headers { get; } = null;
    public string Key { get; }
    public MultipartValueGroup(
        MultibyteEotReader crlfDetector,
        UntilBoundaryReader boundaryDetector,
        MultipartHeader[]? headers = null)
    {
        this.CrlfDetector = crlfDetector;
        this.BoundaryDetector = boundaryDetector;
        this.Key = "";
        if (TryConsumeHeaders(ref headers, out var fieldName))
        {
            this.Key = fieldName;
            this.Headers = headers;
        }
    }
    private bool TryConsumeHeaders([NotNullWhen(true)] ref MultipartHeader[]? headers,
        [NotNullWhen(true)] out string? fieldName)
    {
        headers ??= new MultipartHeaderCollection(CrlfDetector).ToArray();
        var cdHeader = headers.FirstOrDefault(x =>
            x.HeaderName.Equals("content-disposition", StringComparison.OrdinalIgnoreCase));
        if (cdHeader == null || !cdHeader.HeaderArgs.TryGetValue("name", out fieldName) ||
            string.IsNullOrWhiteSpace(fieldName))
            fieldName = null;
        return fieldName != null;
    }
    public IEnumerator<object> GetEnumerator()
    {
        while (true)
        {
            if (Headers == null) yield break;
            yield return new TaggedReader(BoundaryDetector) { Tag = Headers };

            if (!BoundaryDetector.AtEnd)
                yield break;
            BoundaryDetector.Reset();
            if (CrlfDetector.ToAscii().Length != 0)
            {
                CrlfDetector.Reset();
                yield break;
            }
            MultipartHeader[]? newHeaders = null;
            if (!TryConsumeHeaders(ref newHeaders, out var newFieldName))
                yield break;
            if (newFieldName != this.Key)
            {
                this.NextGroup = new MultipartValueGroup(CrlfDetector, BoundaryDetector, newHeaders);
                yield break;
            }
        }
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}