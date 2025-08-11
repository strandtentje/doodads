namespace Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;

public interface IProbeContentLengthInteraction : IInteraction
{
    void AddContentLength(long amount);
    void SetContentLength(long total);
    bool TryGetContentLength(out long contentLength);
    
}

public interface IProbeContentTypeInteraction : IInteraction
{
    void PushContentType(string contentType);
    bool TryGetContentType(out string contentType);
}

public delegate bool TryHandleContentType(out string contentType);
public delegate bool TryHandleContentLength(out long contentLength);