namespace Ziewaar.Web.Uploadserver.Interfaces;

public interface IMultipartComponentResult : IDisposable
{
    string Name { get; }
    void GiveBinaryData(byte[] data, int start, int length);
    IMultipartFinishedComponentResult? Finish();
    string? Fault { get; }
    bool IsExpectingContent { get; }
}