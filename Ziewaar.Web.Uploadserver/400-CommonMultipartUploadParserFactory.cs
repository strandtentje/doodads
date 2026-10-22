namespace Ziewaar.Web.Uploadserver;

public class CommonMultipartUploadParserFactory : IMultipartUploadParserFactory
{
    private readonly IUploadValidator UploadValidator;
    public CommonMultipartUploadParserFactory(
        IUploadValidator uploadValidator)
    {
        this.UploadValidator = uploadValidator;
    }
    public IMultipartUploadParser Create(
        byte[] boundary, string targetDirectory, string[] acceptedFields)
    {
        var forReceiving = FixedBufferPool.CreatePreAllocated(6, 2 * 1024);
        var forProcessing = new FixedBufferPool(6);
        var ingestor = new CommonMultipartComponentIngestor(
            boundary, targetDirectory, acceptedFields, UploadValidator);
        return new CommonMultipartUploadParser(
            forReceiving, forProcessing, ingestor);
    }
}