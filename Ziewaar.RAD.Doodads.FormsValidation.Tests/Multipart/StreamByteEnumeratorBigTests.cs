using Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.StreamingUrlEncoded;

namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Multipart;

[TestClass]
public class StreamByteEnumeratorBigTests
{
    [TestMethod]
    public void StreamByteEnumerator_HandlesLargeStreamCorrectly()
    {
        byte fill = 123;
        var data = Enumerable.Repeat(fill, 10_000_000).Select(b => (byte)b).ToArray();
        using var stream = new MemoryStream(data);
        var enumerator = new StreamByteEnumerator(stream);

        long count = 0;
        while (enumerator.MoveNext())
        {
            Assert.AreEqual(fill, enumerator.Current);
            count++;
        }

        Assert.AreEqual(data.Length, count);
    }

    [TestMethod]
    public void StreamByteEnumerator_StopsAfterStreamEnd()
    {
        var data = new byte[] { 1, 2, 3 };
        using var stream = new MemoryStream(data);
        var enumerator = new StreamByteEnumerator(stream);

        int count = 0;
        while (enumerator.MoveNext()) count++;

        Assert.AreEqual(data.Length, count);
        Assert.IsFalse(enumerator.MoveNext());
    }
}