using System.Text;
using Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.StreamingUrlEncoded;

namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Multipart
{
    [TestClass]
    public class BoundaryMatcherTests
    {
        [TestMethod]
        public void Matcher_CompletesMatch()
        {
            var boundary = Encoding.ASCII.GetBytes("XYZ");
            var matcher = new BoundaryMatcher(boundary);

            bool matched = false;
            foreach (var b in boundary)
            {
                Assert.IsFalse(matched);
                matched = matcher.TryCompleteMatch(b, out var result) && result != MatchResult.NoMatch;
            }
            
            Assert.IsTrue(matched);
        }

        [TestMethod]
        public void Matcher_DetectsPartialThenMatchComplete()
        {
            var matcher = new BoundaryMatcher(Encoding.ASCII.GetBytes("abc"));
            matcher.TryCompleteMatch((byte)'a', out var r1);
            matcher.TryCompleteMatch((byte)'b', out var r2);
            matcher.TryCompleteMatch((byte)'c', out var r3);

            Assert.AreEqual(MatchResult.Matching, r1);
            Assert.AreEqual(MatchResult.Matching, r2);
            Assert.AreEqual(MatchResult.MatchComplete, r3);
        }

        [TestMethod]
        public void Matcher_ResetsOnWrongByte()
        {
            var matcher = new BoundaryMatcher(Encoding.ASCII.GetBytes("ab"));
            matcher.TryCompleteMatch((byte)'a', out var _);
            matcher.TryCompleteMatch((byte)'x', out var r);
            Assert.AreEqual(MatchResult.NoMatch, r);
        }
    }
}