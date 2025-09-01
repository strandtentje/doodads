using Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.StreamingUrlEncoded;

namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Multipart;

[TestClass]
public class SingleValueEnumeratorBigTests
{
    [TestMethod]
    public void Enumerator_CanReturnHugeString()
    {
        string hugeString = new('x', 5_000_000);
        var enumerator = new SingleValueEnumerator<string>(hugeString);

        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreEqual(hugeString, enumerator.Current);
        Assert.IsFalse(enumerator.MoveNext());
    }

    [TestMethod]
    public void Enumerator_CanHoldLargeObjectWithoutLeak()
    {
        var bigData = new byte[10_000_000]; // 10 MB
        var enumerator = new SingleValueEnumerator<byte[]>(bigData);

        GC.Collect(); // Should not collect it early
        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreSame(bigData, enumerator.Current);
    }
}