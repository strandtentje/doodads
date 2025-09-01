using System.Text;
using Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.StreamingUrlEncoded;

namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Multipart;

[TestClass]
public class BoundaryMatcherBigTests
{
    [TestMethod]
    public void Matcher_MatchesLargeBoundary()
    {
        var boundary = Enumerable.Repeat((byte)'A', 4096).ToArray(); // 4KB pattern
        var matcher = new BoundaryMatcher(boundary);

        MatchResult result = MatchResult.NoMatch;
        foreach (var b in boundary)
        {
            matcher.TryCompleteMatch(b, out result);
        }

        Assert.AreEqual(MatchResult.MatchComplete, result);
    }

    [TestMethod]
    public void Matcher_SurvivesHighEntropyNoise()
    {
        var rnd = new Random(42);
        var noise = new byte[1_000_000];
        rnd.NextBytes(noise);

        var pattern = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF };
        var matcher = new BoundaryMatcher(pattern);

        MatchResult result = MatchResult.NoMatch;
        foreach (var b in noise)
        {
            matcher.TryCompleteMatch(b, out result);
            if (result == MatchResult.MatchComplete)
                break; // extremely unlikely
        }

        Assert.AreNotEqual(MatchResult.MatchComplete, result); // sanity check: should not match
    }

    [TestMethod]
    public void Matcher_ResetAfterFalseStart()
    {
        var pattern = Encoding.ASCII.GetBytes("abababx");
        var input = Encoding.ASCII.GetBytes("abababz");

        var matcher = new BoundaryMatcher(pattern);
        MatchResult result = MatchResult.NoMatch;

        foreach (var b in input)
            matcher.TryCompleteMatch(b, out result);

        Assert.AreNotEqual(MatchResult.MatchComplete, result);
    }
}