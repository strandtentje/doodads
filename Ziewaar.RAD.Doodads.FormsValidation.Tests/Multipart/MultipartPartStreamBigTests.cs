using System.Text;
using Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.StreamingUrlEncoded;

namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Multipart;

[TestClass]
public class MultipartPartStreamBigTests
{
    [TestMethod]
    public void Read_LargeBodyWithBoundaryAtEnd()
    {
        string body = new string('x', 5_000_000);
        string boundary = "--ENDOFLINE";
        var data = Encoding.ASCII.GetBytes(body + boundary);

        using var baseStream = new MemoryStream(data);
        using var partStream = new MultipartPartStream(baseStream, Encoding.ASCII.GetBytes(boundary));

        int total = 0;
        var buffer = new byte[8192];
        int read;
        while ((read = partStream.Read(buffer, 0, buffer.Length)) > 0)
            total += read;

        Assert.AreEqual(5_000_000, total);
    }

    [TestMethod]
    public void Read_ReadsInSmallChunksCorrectly()
    {
        string body = new string('y', 1024);
        string boundary = "--B";
        var data = Encoding.ASCII.GetBytes(body + boundary);

        using var baseStream = new MemoryStream(data);
        using var partStream = new MultipartPartStream(baseStream, Encoding.ASCII.GetBytes(boundary));

        int count = 0;
        var buffer = new byte[7];
        int read;
        while ((read = partStream.Read(buffer, 0, buffer.Length)) > 0)
            count += read;

        Assert.AreEqual(1024, count);
    }

    [TestMethod]
    public void Read_ExcessBoundaryStartInStream_DoesNotConfuse()
    {
        string body = "--NotActuallyABoundary--\r\nThis is body\r\n--ActualBoundary";
        var data = Encoding.ASCII.GetBytes(body);
        var part = new MultipartPartStream(new MemoryStream(data), Encoding.ASCII.GetBytes("--ActualBoundary"));

        var buf = new byte[1024];
        int read = part.Read(buf, 0, buf.Length);
        string result = Encoding.ASCII.GetString(buf, 0, read);

        Assert.IsTrue(result.Contains("This is body"));
        Assert.IsFalse(result.Contains("--ActualBoundary"));
    }
}