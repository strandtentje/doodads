using System.Text;
using Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.StreamingUrlEncoded;

namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Multipart;

[TestClass]
public class MultipartPartStreamTests
{
    [TestMethod]
    public void Read_ReturnsStreamUpToBoundary()
    {
        byte[] data = Encoding.ASCII.GetBytes("hello world\r\n--XYZ");
        byte[] boundary = Encoding.ASCII.GetBytes("--XYZ");

        using var baseStream = new MemoryStream(data);
        using var partStream = new MultipartPartStream(baseStream, boundary);

        var buffer = new byte[100];
        int read = partStream.Read(buffer, 0, buffer.Length);

        var result = Encoding.ASCII.GetString(buffer, 0, read);
        Assert.AreEqual("hello world\r\n", result);
    }

    [TestMethod]
    public void Read_ReturnsZeroAfterBoundary()
    {
        byte[] data = Encoding.ASCII.GetBytes("test--XYZ");
        byte[] boundary = Encoding.ASCII.GetBytes("--XYZ");

        using var baseStream = new MemoryStream(data);
        using var partStream = new MultipartPartStream(baseStream, boundary);

        var buf = new byte[100];
        partStream.Read(buf, 0, buf.Length); // consume it all

        int secondRead = partStream.Read(buf, 0, buf.Length);
        Assert.AreEqual(0, secondRead);
    }

    [TestMethod]
    public void MultipartPartStream_ThrowsOnWrite()
    {
        using var baseStream = new MemoryStream();
        var partStream = new MultipartPartStream(baseStream, Encoding.ASCII.GetBytes("--B"));

        Assert.IsFalse(partStream.CanWrite);
        Assert.ThrowsException<NotSupportedException>(() =>
        {
            partStream.Write(new byte[1], 0, 1);
        });
    }
}