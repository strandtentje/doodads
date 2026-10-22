namespace Ziewaar.Web.Uploadserver.Interfaces;

public interface IUploadValidator
{
    bool TryValidate(string contentType, int fileNumber);
}