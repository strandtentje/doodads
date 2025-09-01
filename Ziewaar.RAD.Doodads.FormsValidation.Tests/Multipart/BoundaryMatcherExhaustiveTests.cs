using System.Text;
using Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.StreamingUrlEncoded;

namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Multipart;

[TestClass]
public class BoundaryMatcherExhaustiveTests
{
    [TestMethod]
    public void Matcher_FindsExactMatchAnywhereInStream()
    {
        var pattern = Encoding.ASCII.GetBytes("END");
        var stream = Encoding.ASCII.GetBytes("STARTMIDENDHERE");

        var matcher = new BoundaryMatcher(pattern);
        MatchResult result = MatchResult.NoMatch;

        foreach (var b in stream)
        {
            matcher.TryCompleteMatch(b, out result);
            if (result == MatchResult.MatchComplete)
                break;
        }

        Assert.AreEqual(MatchResult.MatchComplete, result);
    }

    [TestMethod]
    public void Matcher_RecoversFromFalseStarts()
    {
        // pattern "ababc" will have prefix table [0, 0, 1, 2, 0]
        var matcher = new BoundaryMatcher(Encoding.ASCII.GetBytes("ababc"));
        var input = Encoding.ASCII.GetBytes("ababababc");

        MatchResult result = MatchResult.NoMatch;
        foreach (var b in input)
            matcher.TryCompleteMatch(b, out result);

        Assert.AreEqual(MatchResult.MatchComplete, result);
    }

    [TestMethod]
    public void Matcher_HandlesOverlappingMatches()
    {
        var pattern = Encoding.ASCII.GetBytes("aaa");
        var matcher = new BoundaryMatcher(pattern);
        var input = Encoding.ASCII.GetBytes("aaaaa");

        var matchCount = 0;
        MatchResult result;
        foreach (var b in input)
        {
            matcher.TryCompleteMatch(b, out result);
            if (result == MatchResult.MatchComplete)
                matchCount++;
        }

        Assert.AreEqual(1, matchCount); // Only one complete match
    }

    [TestMethod]
    public void Matcher_DoesNotReportPrematureMatches()
    {
        var pattern = Encoding.ASCII.GetBytes("abcde");
        var input = Encoding.ASCII.GetBytes("abc");

        var matcher = new BoundaryMatcher(pattern);
        MatchResult result = MatchResult.NoMatch;

        foreach (var b in input)
            matcher.TryCompleteMatch(b, out result);

        Assert.AreNotEqual(MatchResult.MatchComplete, result);
    }
}