namespace Ziewaar.Web.Uploadserver.FieldResults.Unfinished;

public class MultipartFinishedResult : IMultipartComponentResult
{
    private readonly string _backingFault;
    public MultipartFinishedResult(IEnumerable<string> faults)
    {
        if (faults == null)
            return;
        var stringFaults = faults.
            OfType<string>().Select(x => x.Trim()).
            Where(x => !string.IsNullOrEmpty(x)).
            ToArray();
        _backingFault = string.Join("\r\n", stringFaults);
    }

    public string Name
    {
        get { return "FINISH"; }
    }

    public void GiveBinaryData(byte[] data, int start, int length)
    {

    }

    public IMultipartFinishedComponentResult Finish()
    {
        return null;
    }

    public string Fault
    {
        get { return _backingFault; }
    }

    public bool IsExpectingContent
    {
        get { return false; }
    }

    public void Dispose()
    {
            
    }
}