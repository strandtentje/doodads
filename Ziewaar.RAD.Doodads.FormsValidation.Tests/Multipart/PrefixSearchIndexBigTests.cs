using Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.StreamingUrlEncoded;

namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Multipart;

[TestClass]
public class PrefixSearchIndexBigTests
{
    [TestMethod]
    public void Build_HugeSelfRepeatingPattern()
    {
        var pattern = Enumerable.Repeat((byte)'x', 1_000_000).ToArray();
        var table = PrefixSearchIndex.Build(pattern);

        // Should have ascending prefix counts
        for (int i = 0; i < table.Length; i++)
        {
            Assert.AreEqual(i, table[i]);
        }
    }

    [TestMethod]
    public void Build_HugeABPattern()
    {
        var pattern = new byte[1_000_000];
        for (int i = 0; i < pattern.Length; i++)
            pattern[i] = (byte)((i % 2 == 0) ? 'a' : 'b');

        var table = PrefixSearchIndex.Build(pattern);

        // It should not throw or hang
        Assert.AreEqual(pattern.Length, table.Length);
    }
}