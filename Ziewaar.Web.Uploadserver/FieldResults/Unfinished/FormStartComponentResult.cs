namespace Ziewaar.Web.Uploadserver.FieldResults.Unfinished;

public class FormStartComponentResult : IMultipartComponentResult
{
    private string? _fault = null;
    public string Name => "START";
    public string? Fault => _fault;
    public void GiveBinaryData(byte[] data, int start, int length) => _fault = "Data before form start";
    public IMultipartFinishedComponentResult? Finish() => null;
    public void Dispose() { }
    public bool IsExpectingContent => false;
}