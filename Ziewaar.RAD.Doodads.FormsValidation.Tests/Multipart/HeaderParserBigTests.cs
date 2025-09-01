using System.Text;
using Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.StreamingUrlEncoded;

namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Multipart;

[TestClass]
public class HeaderParserBigTests
{
    [TestMethod]
    public void ParseHeaders_HugeHeaderBlock_DoesNotCrash()
    {
        var sb = new StringBuilder();
        for (int i = 0; i < 10_000; i++)
        {
            sb.Append($"X-Header-{i}: {new string('a', 50)}\r\n");
        }
        sb.Append("Content-Disposition: form-data; name=\"big\"\r\n\r\n");

        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(sb.ToString()));
        var headers = HeaderParser.ParseHeaders(stream);

        Assert.AreEqual("form-data; name=\"big\"", headers["Content-Disposition"]);
        Assert.AreEqual("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", headers["X-Header-0"]);
        Assert.AreEqual(10_001, headers.Count);
    }

    [TestMethod]
    public void ParseHeaders_GarbageFuzzHeaders_ThrowsOnJunk()
    {
        string garbage = string.Join("\r\n", Enumerable.Range(0, 1000).Select(i => $"Header{i} {new string('x', 100)}")) + "\r\n\r\n";
        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(garbage));

        Assert.ThrowsException<InvalidDataException>(() => HeaderParser.ParseHeaders(stream));
    }

    [TestMethod]
    public void ParseHeaders_MalformedFoldingLine_Throws()
    {
        string badFold = "Content-Disposition: form-data;\r\n\tname=weird\r\nContent-Type text/plain\r\n\r\n";
        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(badFold));

        Assert.ThrowsException<InvalidDataException>(() => HeaderParser.ParseHeaders(stream));
    }
}