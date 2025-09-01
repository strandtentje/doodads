using System.Text;
using Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.StreamingUrlEncoded;

namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Multipart;

[TestClass]
public class MultipartBoundaryFinderTests
{
    [TestMethod]
    public void SkipToFirstBoundary_WorksCorrectly()
    {
        string payload = "--foobar\r\nsomething";
        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(payload));

        bool found = stream.SkipToFirstBoundary(Encoding.ASCII.GetBytes("foobar"));
        Assert.IsTrue(found);
    }

    [TestMethod]
    public void ExpectNextBoundary_WorksCorrectly()
    {
        string payload = "\r\n--next\r\nwhatever";
        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(payload));

        bool found = stream.ExpectNextBoundary(Encoding.ASCII.GetBytes("next"));
        Assert.IsTrue(found);
    }

    [TestMethod]
    public void SkipToFirstBoundary_ReturnsFalseWhenMissing()
    {
        using var stream = new MemoryStream(Encoding.ASCII.GetBytes("no boundary here"));

        bool found = stream.SkipToFirstBoundary(Encoding.ASCII.GetBytes("foobar"));
        Assert.IsFalse(found);
    }
}