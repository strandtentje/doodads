namespace Ziewaar.Web.Uploadserver;

public class CommonMultipartUploadParserFactory : IMultipartUploadParserFactory
{
    private readonly IContentTypeValidator ContentTypeValidator;
    public CommonMultipartUploadParserFactory(
        IContentTypeValidator contentTypeValidator)
    {
        this.ContentTypeValidator = contentTypeValidator;
    }
    public IMultipartUploadParser Create(
        byte[] boundary, string targetDirectory, string[] acceptedFields)
    {
        var forReceiving = FixedBufferPool.CreatePreAllocated(6, 2 * 1024);
        var forProcessing = new FixedBufferPool(6);
        var ingestor = new CommonMultipartComponentIngestor(
            boundary, targetDirectory, acceptedFields, ContentTypeValidator);
        return new CommonMultipartUploadParser(
            forReceiving, forProcessing, ingestor);
    }
}