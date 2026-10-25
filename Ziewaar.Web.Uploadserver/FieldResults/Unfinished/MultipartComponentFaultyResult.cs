namespace Ziewaar.Web.Uploadserver.FieldResults.Unfinished;

public class MultipartComponentFaultyResult(string fault) : IMultipartComponentResult
{
    public string Name { get { return "FAULT"; } }
    public string Fault { get { return fault; } }

    public void GiveBinaryData(byte[] data, int start, int length)
    {

    }

    public IMultipartFinishedComponentResult? Finish() => null;

    public void Dispose()
    {
    }

    public bool IsExpectingContent => false;
}