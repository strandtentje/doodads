using System.Text;
using Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.StreamingUrlEncoded;

namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Multipart;

[TestClass]
public class StreamBoundarySlicerTests
{
    [TestMethod]
    public void ReadUntilDelimiter_StopsAtBoundary()
    {
        byte[] content = Encoding.ASCII.GetBytes("abc--myboundary");
        byte[] boundary = Encoding.ASCII.GetBytes("--myboundary");

        using var stream = new MemoryStream(content);
        var slicer = new StreamBoundarySlicer(stream, boundary);

        byte[] buffer = new byte[100];
        int read = slicer.ReadUntilDelimiter(buffer, 0, buffer.Length);

        string result = Encoding.ASCII.GetString(buffer, 0, read);
        Assert.AreEqual("abc", result);
    }

    [TestMethod]
    public void ReadUntilDelimiter_ReturnsPartialReadIfShort()
    {
        byte[] content = Encoding.ASCII.GetBytes("abc--boundary");
        byte[] boundary = Encoding.ASCII.GetBytes("--boundary");

        using var stream = new MemoryStream(content);
        var slicer = new StreamBoundarySlicer(stream, boundary);

        byte[] buffer = new byte[2];
        int read = slicer.ReadUntilDelimiter(buffer, 0, buffer.Length);

        Assert.AreEqual(2, read);
    }

    [TestMethod]
    public void ReadUntilDelimiter_ReturnsZeroAfterEndReached()
    {
        byte[] content = Encoding.ASCII.GetBytes("abc--b");
        byte[] boundary = Encoding.ASCII.GetBytes("--b");

        using var stream = new MemoryStream(content);
        var slicer = new StreamBoundarySlicer(stream, boundary);

        var buffer = new byte[10];
        slicer.ReadUntilDelimiter(buffer, 0, 10); // consume
        int second = slicer.ReadUntilDelimiter(buffer, 0, 10);

        Assert.AreEqual(0, second);
    }
}