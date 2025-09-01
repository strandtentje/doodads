using System.Text;
using Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.StreamingUrlEncoded;

namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Multipart;

[TestClass]
public class PartReaderBigTests
{
    [TestMethod]
    public void ReadNextPart_ManyPartsInLargeStream()
    {
        var sb = new StringBuilder();
        for (int i = 0; i < 31; i++)
        {
            sb.Append("--boundary\r\nContent-Disposition: form-data; name=\"x" + i + "\"\r\n\r\nval" + i + "\r\n");
        }
        sb.Append("--boundary");

        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(sb.ToString()));
        var reader = new PartReader(stream, Encoding.ASCII.GetBytes("boundary"));

        int count = 0;
        while (reader.ReadNextPart() is MultipartPayload mpp)
        {
            using var ms = new MemoryStream();
            mpp.Body.CopyTo(ms); // force reading to boundary
            count++;
        }

        Assert.AreEqual(31, count);
    }

    [TestMethod]
    public void ReadNextPart_LongBoundaryDoesNotConfuse()
    {
        string longBoundary = new string('X', 2000);
        string payload = "--" + longBoundary + "\r\nContent-Disposition: form-data; name=\"x\"\r\n\r\n123\r\n--" + longBoundary;

        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(payload));
        var reader = new PartReader(stream, Encoding.ASCII.GetBytes(longBoundary));

        var part = reader.ReadNextPart();
        Assert.IsNotNull(part);
        Assert.AreEqual("x", part.Header.Name);
    }
    [TestMethod]
    public void ParseHeaders_SkipsBlankLinesBeforeHeaders()
    {
        string headers = "\r\n\r\nContent-Disposition: form-data; name=\"abc\"\r\n\r\n";
        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(headers));

        var parsed = HeaderParser.ParseHeaders(stream);
        Assert.AreEqual("form-data; name=\"abc\"", parsed["Content-Disposition"]);
    }

    [TestMethod]
    public void ParseHeaders_TooManyLines_Throws()
    {
        var sb = new StringBuilder();
        for (int i = 0; i < 1000; i++)
            sb.Append($"Header-{i}: val\r\n");
        sb.Append("\r\n");

        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(sb.ToString()));
        Assert.ThrowsException<InvalidDataException>(() => HeaderParser.ParseHeaders(stream));
    }

    [TestMethod]
    public void ParseHeaders_InsaneHeaderLine_Throws()
    {
        string line = "X: " + new string('A', 9000) + "\r\n\r\n";
        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(line));

        Assert.ThrowsException<InvalidDataException>(() => HeaderParser.ParseHeaders(stream));
    }
}