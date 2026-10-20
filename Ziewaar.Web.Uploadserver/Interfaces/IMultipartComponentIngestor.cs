namespace Ziewaar.Web.Uploadserver.Interfaces;

public interface IMultipartComponentIngestor : IDisposable
{
    bool IsExpectingContent { get; }

    bool TryHaveBoundary(out byte[] boundary);
    void GiveBinaryData(byte[] bytes, int start, int count);

    bool TryRetireBoundary();

    bool GiveHeaderString(string headerString);

    void Finish(
        IDictionary<string, IMultipartFinishedComponentResult> values,
        IList<string> faults);
}