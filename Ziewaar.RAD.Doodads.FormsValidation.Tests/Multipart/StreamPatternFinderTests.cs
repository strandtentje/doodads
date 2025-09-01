using System.Text;
using Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.StreamingUrlEncoded;

namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Multipart;

[TestClass]
public class StreamPatternFinderTests
{
    [TestMethod]
    public void Seek_FindsPatternCorrectly()
    {
        var content = Encoding.ASCII.GetBytes("xxx--boundaryyyy");
        using var stream = new MemoryStream(content);

        bool found = StreamPatternFinder.Seek(stream, Encoding.ASCII.GetBytes("--boundary"));
        Assert.IsTrue(found);
        Assert.AreEqual((int)'y', stream.ReadByte()); // Should be after boundary
    }

    [TestMethod]
    public void Seek_SkipsGarbageBeforePattern()
    {
        string input = new string('a', 500) + "--b";
        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(input));

        bool found = StreamPatternFinder.Seek(stream, Encoding.ASCII.GetBytes("--b"));
        Assert.IsTrue(found);
    }

    [TestMethod]
    public void Seek_ReturnsFalseIfPatternNotFound()
    {
        using var stream = new MemoryStream(Encoding.ASCII.GetBytes("nope nope nope"));

        bool found = StreamPatternFinder.Seek(stream, Encoding.ASCII.GetBytes("needle"));
        Assert.IsFalse(found);
    }
}