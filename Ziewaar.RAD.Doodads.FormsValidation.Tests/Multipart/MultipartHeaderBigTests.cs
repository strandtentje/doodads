using System.Text;
using Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.StreamingUrlEncoded;

namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Multipart;

[TestClass]
public class MultipartHeaderBigTests
{
    [TestMethod]
    public void Parse_VeryLargeHeaderWithFuzzedParams()
    {
        var sb = new StringBuilder();
        sb.Append("Content-Disposition: form-data; name=\"bigname\"; filename=\"file.txt\"\r\n");
        for (int i = 0; i < 5000; i++)
        {
            sb.Append($"X-Fuzz-{i}: {Guid.NewGuid()}\r\n");
        }
        sb.Append("\r\n");

        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(sb.ToString()));
        var header = MultipartHeader.Parse(stream);

        Assert.AreEqual("bigname", header.Name);
        Assert.AreEqual("file.txt", header.FileName);
        Assert.AreEqual(5001, header.Headers.Count);
    }

    [TestMethod]
    public void Parse_MissingDisposition_Throws()
    {
        string bad = "X-Foo: bar\r\n\r\n";
        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(bad));
        Assert.ThrowsException<InvalidDataException>(() => MultipartHeader.Parse(stream));
    }

    [TestMethod]
    public void Parse_MalformedNameParam_Throws()
    {
        string bad = "Content-Disposition: form-data; badname=oops\r\n\r\n";
        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(bad));
        Assert.ThrowsException<InvalidDataException>(() => MultipartHeader.Parse(stream));
    }
}