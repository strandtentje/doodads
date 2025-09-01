using System.Text;
using Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.StreamingUrlEncoded;

namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Multipart;

[TestClass]
public class MultipartHeaderTests
{
    [TestMethod]
    public void Parse_ParsesNameAndFilenameCorrectly()
    {
        string headers = "Content-Disposition: form-data; name=\"myname\"; filename=\"myfile.txt\"\r\nX-Custom: abc\r\n\r\n";
        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(headers));

        var header = MultipartHeader.Parse(stream);

        Assert.AreEqual("myname", header.Name);
        Assert.AreEqual("myfile.txt", header.FileName);
        Assert.AreEqual("abc", header.Headers["X-Custom"]);
    }

    [TestMethod]
    public void Parse_ThrowsIfMissingContentDisposition()
    {
        string headers = "X-Other: something\r\n\r\n";
        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(headers));

        Assert.ThrowsException<InvalidDataException>(() => MultipartHeader.Parse(stream));
    }

    [TestMethod]
    public void Parse_ThrowsIfMissingNameParameter()
    {
        string headers = "Content-Disposition: form-data\r\n\r\n";
        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(headers));

        Assert.ThrowsException<InvalidDataException>(() => MultipartHeader.Parse(stream));
    }
}