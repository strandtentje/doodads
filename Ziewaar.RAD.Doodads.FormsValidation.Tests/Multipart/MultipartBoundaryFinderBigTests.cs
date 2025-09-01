using System.Text;
using Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.StreamingUrlEncoded;

namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Multipart;

[TestClass]
public class MultipartBoundaryFinderBigTests
{
    [TestMethod]
    public void SkipToFirstBoundary_HugeStreamStillFindsIt()
    {
        var noise = new string('x', 3_000_000);
        var boundary = "BOUNDARY123";
        var stream = new MemoryStream(Encoding.ASCII.GetBytes(noise + "--" + boundary));

        bool found = stream.SkipToFirstBoundary(Encoding.ASCII.GetBytes(boundary));
        Assert.IsTrue(found);
    }

    [TestMethod]
    public void ExpectNextBoundary_CanHandleEdgeNewlines()
    {
        var payload = "\r\n--longboundary\r\n";
        var stream = new MemoryStream(Encoding.ASCII.GetBytes(payload));

        bool found = stream.ExpectNextBoundary(Encoding.ASCII.GetBytes("longboundary"));
        Assert.IsTrue(found);
    }
}