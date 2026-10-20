namespace Ziewaar.Web.Uploadserver.Interfaces;

public interface IUploadSizeGovernor
{
    bool ValidateContentLength(string directory, long contentLength);
}