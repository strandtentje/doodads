namespace Ziewaar.Web.Uploadserver.FieldResults.Unfinished;

public class FormStartComponentResult : IMultipartComponentResult
{
    private string _fault = null;
    public string Name { get { return "START"; } }
    public string Fault { get { return _fault; } }

    public void GiveBinaryData(byte[] data, int start, int length)
    {
        this._fault = "Data before form start";
    }

    public IMultipartFinishedComponentResult Finish()
    {
        return null;
    }

    public void Dispose()
    {

    }

    public bool IsExpectingContent { get { return false; } }
}