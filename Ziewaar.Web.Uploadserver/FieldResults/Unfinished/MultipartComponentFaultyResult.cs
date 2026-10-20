namespace Ziewaar.Web.Uploadserver.FieldResults.Unfinished;

public class MultipartComponentFaultyResult : IMultipartComponentResult
{
    private readonly string _fault;
    public string Name { get { return "FAULT"; } }
    public string Fault { get { return _fault; } }
    public MultipartComponentFaultyResult(string fault)
    {
        this._fault = fault;
    }

    public void GiveBinaryData(byte[] data, int start, int length)
    {

    }

    public IMultipartFinishedComponentResult Finish()
    {
        return null;
    }

    public void Dispose()
    {
    }

    public bool IsExpectingContent
    {
        get { return false; }
    }
}