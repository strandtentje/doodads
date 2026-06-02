using Ziewaar.RAD.Doodads.AdvancedFilesystem.DirDsl;

namespace Ziewaar.DirDsl.Tests
{
    [TestClass]
    public class BooleanSeriesParseTest
    {
        private readonly string
            OR_SERIES = ".'abc','def'",
            AND_SERIES = "&$'abc',;'def'",
            SAME_SERIES = "=ß'abc',ß'def'",
            XOR_SERIES = "@ß'abc',~'def'";

        [TestMethod]
        public void TestOr()
        {
            var errors = new List<(int c, string d)>();
            int cursor = 0;
            Assert.IsTrue(BooleanSeries.TryParseFrom(OR_SERIES, errors, ref cursor, out var orCandidate));
            var orExp = Assert.IsInstanceOfType<OrBooleanSeries>(orCandidate);
            Assert.HasCount(2, orExp.Expressions);
            Assert.IsInstanceOfType<EqualsCondition>(orExp.Expressions[0]);
            Assert.IsInstanceOfType<EqualsCondition>(orExp.Expressions[1]);
        }
        [TestMethod]
        public void TestAnd()
        {
            var errors = new List<(int c, string d)>();
            int cursor = 0;
            Assert.IsTrue(BooleanSeries.TryParseFrom(AND_SERIES, errors, ref cursor, out var orCandidate));
            var orExp = Assert.IsInstanceOfType<AndBooleanSeries>(orCandidate);
            Assert.HasCount(2, orExp.Expressions);
            Assert.IsInstanceOfType<StartsWithCondition>(orExp.Expressions[0]);
            Assert.IsInstanceOfType<EndsWithCondition>(orExp.Expressions[1]);
        }
        [TestMethod]
        public void TestSame()
        {
            var errors = new List<(int c, string d)>();
            int cursor = 0;
            Assert.IsTrue(BooleanSeries.TryParseFrom(SAME_SERIES, errors, ref cursor, out var orCandidate));
            var orExp = Assert.IsInstanceOfType<AllSameBooleanSeries>(orCandidate);
            Assert.HasCount(2, orExp.Expressions);
            Assert.IsInstanceOfType<ContainsCondition>(orExp.Expressions[0]);
            Assert.IsInstanceOfType<ContainsCondition>(orExp.Expressions[1]);
        }
        [TestMethod]
        public void TestXor()
        {
            var errors = new List<(int c, string d)>();
            int cursor = 0;
            Assert.IsTrue(BooleanSeries.TryParseFrom(XOR_SERIES, errors, ref cursor, out var orCandidate));
            var orExp = Assert.IsInstanceOfType<XorBooleanSeries>(orCandidate);
            Assert.HasCount(2, orExp.Expressions);
            Assert.IsInstanceOfType<ContainsCondition>(orExp.Expressions[0]);
            Assert.IsInstanceOfType<NotEqualsCondition>(orExp.Expressions[1]);
        }
    }
}
