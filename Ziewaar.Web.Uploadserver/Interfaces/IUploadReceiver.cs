namespace Ziewaar.Web.Uploadserver.Interfaces;

public interface IUploadReceiver
{
    bool TryValidateUpload(string? contentType, long contentLength, UploadBox selectedUploadBox,
        out PreValidationState state);
    bool TryReceiveUpload(Stream inputStream, UploadBox selectedUploadBox);
}