namespace Ziewaar.Web.Uploadserver.Interfaces;

public interface IContentTypeValidator
{
    bool TryValidateContentType(string contentType);
}