using System.Text;
using Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.StreamingUrlEncoded;

namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Multipart;

[TestClass]
public class StreamPatternFinderBigTests
{
    [TestMethod]
    public void Seek_SurvivesMegabytesOfNoise()
    {
        var junk = new string('z', 5_000_000);
        var boundary = "--findme";
        var input = junk + boundary;

        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(input));

        bool found = StreamPatternFinder.Seek(stream, Encoding.ASCII.GetBytes(boundary));
        Assert.IsTrue(found);
    }

    [TestMethod]
    public void Seek_LongBoundaryOverlappingPattern()
    {
        var pattern = "--xyzxyzxyzxyzxyz";
        var input = "xyzxyzxyzxyzxyzxyz--xyzxyzxyzxyzxyz";

        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(input));
        bool found = StreamPatternFinder.Seek(stream, Encoding.ASCII.GetBytes(pattern));

        Assert.IsTrue(found);
    }
}