using System.Text;
using Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.StreamingUrlEncoded;

namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Multipart;

[TestClass]
public class MultipartHeaderExtensionsBigTests
{
    [TestMethod]
    public void GetEncodingOrDefault_HandlesLargeContentType()
    {
        string contentType = "text/plain; charset=utf-8;" + string.Join(";", Enumerable.Range(0, 1000).Select(i => $"x{i}=y{i}"));

        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Content-Type", contentType }
        };

        var encoding = headers.GetEncodingOrDefault(Encoding.ASCII);
        Assert.AreEqual(Encoding.UTF8.WebName, encoding.WebName);
    }

    [TestMethod]
    public void GetEncodingOrDefault_HandlesCompletelyUnknownCharset()
    {
        var headers = new Dictionary<string, string>
        {
            { "Content-Type", "application/json; charset=☃☃☃☃☃" }
        };

        var encoding = headers.GetEncodingOrDefault(Encoding.Unicode);
        Assert.AreEqual(Encoding.Unicode.WebName, encoding.WebName);
    }

    [TestMethod]
    public void GetEncodingOrDefault_FuzzedContentType_DoesNotThrow()
    {
        var fuzz = string.Concat(Enumerable.Repeat("charset=", 100)) + "utf-8";

        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Content-Type", fuzz }
        };

        var encoding = headers.GetEncodingOrDefault(Encoding.Latin1);
        Assert.AreEqual(Encoding.Latin1.WebName, encoding.WebName); // fallback
    }
}