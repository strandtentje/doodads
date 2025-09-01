using Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.StreamingUrlEncoded;

namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Multipart;

[TestClass]
public class PrefixSearchIndexTests
{
    [TestMethod]
    public void Build_SimplePattern_ReturnsCorrectPrefixTable()
    {
        var pattern = new byte[] { (byte)'a', (byte)'b', (byte)'a', (byte)'b', (byte)'c' };
        var expected = new[] { 0, 0, 1, 2, 0 };

        var table = PrefixSearchIndex.Build(pattern);

        CollectionAssert.AreEqual(expected, table);
    }

    [TestMethod]
    public void Build_EmptyPattern_ReturnsEmptyArray()
    {
        var pattern = Array.Empty<byte>();
        var table = PrefixSearchIndex.Build(pattern);

        Assert.AreEqual(0, table.Length);
    }

    [TestMethod]
    public void Build_SelfRepeatingPattern()
    {
        var pattern = new byte[] { (byte)'a', (byte)'a', (byte)'a', (byte)'a' };
        var expected = new[] { 0, 1, 2, 3 };

        var table = PrefixSearchIndex.Build(pattern);

        CollectionAssert.AreEqual(expected, table);
    }
}