using Ziewaar.RAD.Doodads.EnumerableStreaming.Readers;
using Ziewaar.RAD.Doodads.EnumerableStreaming.StreamingUrlEncoded;

namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.FormBodyReading;
[TestClass]
public class UrlEncodedTokenReaderLazyDictionaryIntegrationTests
{
    private static ICountingEnumerator<byte> FromAscii(string s)
        => new RootByteReader(new MemoryStream(System.Text.Encoding.ASCII.GetBytes(s)), limit: -1);
    private static string Drain(ICountingEnumerator<char> r)
    {
        var sb = new System.Text.StringBuilder();
        while (r.CanContinue() && r.MoveNext()) sb.Append(r.Current);
        return sb.ToString();
    }
    [TestMethod]
    public void LazyFormDataDictionary_GroupsConsecutive_AndRejectsNonConsecutive()
    {
        // grouped
        var dict = new StreamingFormDataEnumerable(new UrlEncodedTokenReader(FromAscii("a=1&a=2&b=3")));

        var groups = Explode(dict);

        Assert.AreEqual(2, groups.Count);
        Assert.AreEqual("a", groups[0].Key);
        CollectionAssert.AreEqual(new[] { "1", "2" }, groups[0].Value);
        Assert.AreEqual("b", groups[1].Key);
        CollectionAssert.AreEqual(new[] { "3" }, groups[1].Value);

        // non-consecutive duplicate key should throw
        var bad = new StreamingFormDataEnumerable(new UrlEncodedTokenReader(FromAscii("a=1&b=2&a=3")));
        Assert.ThrowsException<ConsecutiveKeyException>(() => Explode(bad));
    }
    private static List<KeyValuePair<string, string[]>> Explode(StreamingFormDataEnumerable dict)
    {
        List<KeyValuePair<string, string[]>> groups = new();

        foreach (var group in dict)
        {
            List<string> arr = new();
            foreach (var member in group)
            {
                if (((ICountingEnumerator<char>)member).TryRenderToString(out var x))
                {
                    arr.Add(x);
                }
                else
                {
                    Assert.Fail();
                }
            }
            groups.Add(new (group.Key, arr.ToArray()));
        }
        return groups;
    }
}