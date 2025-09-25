using Ziewaar.RAD.Doodads.EnumerableStreaming.Readers;

namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.FormBodyReading;
[TestClass]
public class UrlEncodedValueBytesReaderTests
{
    private static ICountingEnumerator<byte> FromAscii(string s)
        => new RootByteReader(new MemoryStream(System.Text.Encoding.ASCII.GetBytes(s)), limit: -1);

    private static byte[] ReadAll(ICountingEnumerator<byte> r)
    {
        var list = new List<byte>();
        while (r.MoveNext()) list.Add(r.Current);
        return list.ToArray();
    }

    [TestMethod]
    public void Decodes_PlusAndPercent_StopsBeforeDelimiters()
    {
        // "A%42+B&rest" => bytes: 'A','B',' ', 'B' ; stop before '&'
        var bytes = ReadAll(new UrlEncodedValueBytesReader(FromAscii("A%42+B&rest"), limit: -1));
        CollectionAssert.AreEqual(new byte[] { (byte)'A', (byte)'B', (byte)' ', (byte)'B' }, bytes);
    }

    [TestMethod]
    public void StopsOnEquals_AsTerminator()
    {
        var bytes = ReadAll(new UrlEncodedValueBytesReader(FromAscii("val=next"), limit: -1));
        CollectionAssert.AreEqual(System.Text.Encoding.ASCII.GetBytes("val"), bytes);
    }

    [TestMethod]
    public void Rejects_UnsafeAscii()
    {
        // raw space must be '+', so unsafe
        var r = new UrlEncodedValueBytesReader(FromAscii("a b"), limit: -1);
        Assert.IsTrue(r.MoveNext());
        Assert.IsFalse(r.MoveNext());
        Assert.IsNotNull(r.ErrorState);
        Assert.IsFalse(string.IsNullOrEmpty(r.ErrorState));
    }

    [TestMethod]
    public void Limit_Is_Respected()
    {
        var r = new UrlEncodedValueBytesReader(FromAscii("abcdef"), limit: 3);
        var got = new List<byte>();
        while (r.MoveNext()) got.Add(r.Current);

        CollectionAssert.AreEqual(System.Text.Encoding.ASCII.GetBytes("abc"), got);
        Assert.IsFalse(string.IsNullOrEmpty(r.ErrorState));
    }
}