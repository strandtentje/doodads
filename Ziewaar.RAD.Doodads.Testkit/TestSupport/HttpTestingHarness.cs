namespace Ziewaar.RAD.Doodads.Testkit;
public class HttpTestingHarness(
    Task<RkopTestingHarness> run, 
    Stream asStream, 
    TestingHttpResponseInteraction httpRes)
    : IDisposable
{
    public void Wait(TimeSpan ts)
    {
        if (!run.Wait(ts))
            run.Dispose();
    }
    public string ContentType => httpRes.SinkTrueContentType ?? "testing/unknown";
    public long ContentLength => httpRes.ContentLengthReports.LastOrDefault();
    public Stream Stream => asStream;
    public void Dispose()
    {
        run.Dispose();
        asStream.Dispose();
    }
}