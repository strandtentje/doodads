namespace Ziewaar.Web.Uploadserver.Interfaces;

public interface IMultipartUploadParserFactory
{
    IMultipartUploadParser Create(
        byte[] boundary, 
        string targetDirectory,
        string[] acceptedFields);
}