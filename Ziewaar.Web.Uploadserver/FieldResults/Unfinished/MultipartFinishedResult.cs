namespace Ziewaar.Web.Uploadserver.FieldResults.Unfinished;

public class MultipartFinishedResult : IMultipartComponentResult
{
    public MultipartFinishedResult(IEnumerable<string>? faults)
    {
        if (faults == null)
            return;
        var stringFaults = faults.OfType<string>().Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToArray();
        Fault = string.Join("\r\n", stringFaults);
    }

    public string Name => "FINISH";

    public void GiveBinaryData(byte[] data, int start, int length)
    {
    }

    public IMultipartFinishedComponentResult? Finish() => null;
    public string? Fault { get; }
    public bool IsExpectingContent => false;

    public void Dispose()
    {
    }
}