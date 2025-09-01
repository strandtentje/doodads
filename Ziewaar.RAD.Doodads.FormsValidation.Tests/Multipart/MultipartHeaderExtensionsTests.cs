using System.Text;
using Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.StreamingUrlEncoded;

namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Multipart;

[TestClass]
public class MultipartHeaderExtensionsTests
{
    [TestMethod]
    public void GetEncodingOrDefault_ParsesCharsetCorrectly()
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Content-Type", "text/plain; charset=utf-8" }
        };

        var encoding = headers.GetEncodingOrDefault(Encoding.ASCII);

        Assert.AreEqual(Encoding.UTF8.WebName, encoding.WebName);
    }

    [TestMethod]
    public void GetEncodingOrDefault_ReturnsFallbackOnUnknown()
    {
        var headers = new Dictionary<string, string>
        {
            { "Content-Type", "application/json; charset=flarp-xyz" }
        };

        var encoding = headers.GetEncodingOrDefault(Encoding.ASCII);

        Assert.AreEqual(Encoding.ASCII.WebName, encoding.WebName);
    }

    [TestMethod]
    public void GetEncodingOrDefault_IgnoresMissingCharset()
    {
        var headers = new Dictionary<string, string>
        {
            { "Content-Type", "text/plain" }
        };

        var encoding = headers.GetEncodingOrDefault(Encoding.UTF7);

        Assert.AreEqual(Encoding.UTF7.WebName, encoding.WebName);
    }

    [TestMethod]
    public void GetEncodingOrDefault_WorksWithoutContentType()
    {
        var headers = new Dictionary<string, string>();

        var encoding = headers.GetEncodingOrDefault(Encoding.BigEndianUnicode);

        Assert.AreEqual(Encoding.BigEndianUnicode.WebName, encoding.WebName);
    }
}