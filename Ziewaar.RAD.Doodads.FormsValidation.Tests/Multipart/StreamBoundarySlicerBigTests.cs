using System.Text;
using Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.StreamingUrlEncoded;

namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Multipart;

[TestClass]
public class StreamBoundarySlicerBigTests
{
    [TestMethod]
    public void Slicer_HandlesHighVolumeBeforeBoundary()
    {
        var payload = new string('a', 2_000_000) + "--BOUNDARY";
        var stream = new MemoryStream(Encoding.ASCII.GetBytes(payload));
        var slicer = new StreamBoundarySlicer(stream, Encoding.ASCII.GetBytes("--BOUNDARY"));

        var buffer = new byte[8192];
        int total = 0, read;
        while ((read = slicer.ReadUntilDelimiter(buffer, 0, buffer.Length)) > 0)
            total += read;

        Assert.AreEqual(2_000_000, total);
    }

    [TestMethod]
    public void Slicer_SurvivesPartialReadsAndBacktracking()
    {
        var sb = new StringBuilder();
        sb.Append("START--BO");
        for (int i = 0; i < 10_000; i++)
            sb.Append("UNCE"); // cause matcher to reset many times
        sb.Append("UNDARY");

        var data = Encoding.ASCII.GetBytes(sb.ToString());
        var stream = new MemoryStream(data);
        var slicer = new StreamBoundarySlicer(stream, Encoding.ASCII.GetBytes("--BOUNDARY"));

        var buffer = new byte[2048];
        int total = 0;
        int read;
        while ((read = slicer.ReadUntilDelimiter(buffer, 0, buffer.Length)) > 0)
            total += read;

        Assert.IsTrue(total > 0); // didn't crash
        Assert.IsFalse(Encoding.ASCII.GetString(buffer).Contains("--BOUNDARY"));
    }

    [TestMethod]
    public void Slicer_HandlesImmediateBoundary()
    {
        var stream = new MemoryStream(Encoding.ASCII.GetBytes("--BANG"));
        var slicer = new StreamBoundarySlicer(stream, Encoding.ASCII.GetBytes("--BANG"));

        var buffer = new byte[10];
        int read = slicer.ReadUntilDelimiter(buffer, 0, buffer.Length);

        Assert.AreEqual(0, read);
    }
}