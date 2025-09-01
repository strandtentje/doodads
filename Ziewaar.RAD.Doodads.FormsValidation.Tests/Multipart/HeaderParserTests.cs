using System.Text;
using Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.StreamingUrlEncoded;

namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Multipart;

[TestClass]
public class HeaderParserTests
{
    [TestMethod]
    public void ParseHeaders_ParsesBasicHeaderBlock()
    {
        string headers = "Content-Disposition: form-data; name=\"myfield\"\r\nContent-Type: text/plain\r\n\r\n";
        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(headers));

        var parsed = HeaderParser.ParseHeaders(stream);

        Assert.AreEqual("form-data; name=\"myfield\"", parsed["Content-Disposition"]);
        Assert.AreEqual("text/plain", parsed["Content-Type"]);
    }

    [TestMethod]
    public void ParseHeaders_HandlesHeaderFolding()
    {
        string headers = "Content-Disposition: form-data;\r\n name=\"folded\"\r\n\r\n";
        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(headers));

        var parsed = HeaderParser.ParseHeaders(stream);

        Assert.AreEqual("form-data; name=\"folded\"", parsed["Content-Disposition"]);
    }

    [TestMethod]
    public void ParseHeaders_ThrowsOnMalformedHeader()
    {
        string headers = "Content-Disposition this-is-bad\r\n\r\n";
        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(headers));

        Assert.ThrowsException<InvalidDataException>(() => HeaderParser.ParseHeaders(stream));
    }

    [TestMethod]
    public void ExtractParameter_ReturnsUnquotedAndQuotedValues()
    {
        string header = "form-data; name=\"myfield\"; filename=plain.txt";

        Assert.AreEqual("myfield", HeaderParser.ExtractParameter(header, "name"));
        Assert.AreEqual("plain.txt", HeaderParser.ExtractParameter(header, "filename"));
        Assert.IsNull(HeaderParser.ExtractParameter(header, "nonexistent"));
    }
}