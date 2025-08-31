using Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.Readers;

namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.FormBodyReading;
[TestClass]
public class RootByteReaderTests
{
    private static RootByteReader R(params byte[] data) => new RootByteReader(new MemoryStream(data), limit: -1);

    [TestMethod]
    public void ReadsBytes_UntilEnd()
    {
        var r = R((byte)'A', (byte)'B', (byte)'C');
        var bytes = new List<byte>();
        while (r.MoveNext()) bytes.Add(r.Current);

        CollectionAssert.AreEqual(new byte[] { (byte)'A', (byte)'B', (byte)'C' }, bytes);
        Assert.IsTrue(r.AtEnd);
        Assert.IsNull(r.ErrorState);
        Assert.AreEqual(3, r.Cursor);
    }

    [TestMethod]
    public void StopsOnTerminator_ZeroByte()
    {
        var r = new RootByteReader(new MemoryStream(new byte[] { (byte)'X', 0, (byte)'Y' }), limit: -1);
        var bytes = new List<byte>();
        while (r.MoveNext()) bytes.Add(r.Current);

        CollectionAssert.AreEqual(new byte[] { (byte)'X' }, bytes);
        Assert.IsTrue(r.AtEnd);
        Assert.IsNull(r.ErrorState);
        Assert.AreEqual(1, r.Cursor);
    }

    [TestMethod]
    public void LimitEnforced_WhenSet()
    {
        var r = new RootByteReader(new MemoryStream(new byte[] { 1, 2, 3, 4 }), limit: 2);
        var seen = new List<byte>();
        while (r.MoveNext()) seen.Add(r.Current);

        // Should stop at cursor==2 with an error
        CollectionAssert.AreEqual(new byte[] { 1, 2 }, seen);
        Assert.IsFalse(string.IsNullOrEmpty(r.ErrorState));
    }
}