using System.Text;
using Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.StreamingUrlEncoded;

namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Multipart;

[TestClass]
public class PartReaderTests
{
    [TestMethod]
    public void ReadNextPart_SkipsFirstThenReadsPart()
    {
        string payload = "--boundary\r\nContent-Disposition: form-data; name=\"a\"\r\n\r\nval\r\n--boundary";
        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(payload));
        var reader = new PartReader(stream, Encoding.ASCII.GetBytes("boundary"));

        var part = reader.ReadNextPart();
        Assert.IsNotNull(part);
        Assert.AreEqual("a", part.Header.Name);
    }

    [TestMethod]
    public void ReadNextPart_SecondCallReturnsNullOnEOF()
    {
        string payload = "--boundary\r\nContent-Disposition: form-data; name=\"b\"\r\n\r\nval\r\n--boundary";
        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(payload));
        var reader = new PartReader(stream, Encoding.ASCII.GetBytes("boundary"));

        Assert.IsNotNull(reader.ReadNextPart());
        Assert.IsNull(reader.ReadNextPart());
    }
    [TestMethod]
    public void ReadNextPart_SuccessfullyParsesSinglePart()
    {
        string payload =
            "--boundary\r\n" +
            "Content-Disposition: form-data; name=\"test\"\r\n" +
            "\r\n" +
            "hello\r\n" +
            "--boundary";

        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(payload));
        var reader = new PartReader(stream, Encoding.ASCII.GetBytes("boundary"));

        var part = reader.ReadNextPart();
        Assert.IsNotNull(part);
        Assert.AreEqual("test", part.Header.Name);
    }
    [TestMethod]
    public void ReadNextPart_NoStartBoundary_ReturnsNull()
    {
        string payload = "some random junk";
        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(payload));
        var reader = new PartReader(stream, Encoding.ASCII.GetBytes("boundary"));

        Assert.IsNull(reader.ReadNextPart());
    }

    [TestMethod]
    public void ReadNextPart_MalformedHeader_Throws()
    {
        string payload = "--boundary\r\nBad-Header: lol\r\n\r\nvalue\r\n--boundary";
        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(payload));
        var reader = new PartReader(stream, Encoding.ASCII.GetBytes("boundary"));

        Assert.ThrowsException<InvalidDataException>(() => reader.ReadNextPart());
    }
}