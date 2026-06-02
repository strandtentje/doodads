using Ziewaar.RAD.Doodads.AdvancedFilesystem.DirDsl;

namespace Ziewaar.DirDsl.Tests
{
    [TestClass]
    public class LeadingTrailingEvalTest
    {
        private readonly StartsWithCondition SW_SUT = new StartsWithCondition(
            new StringComparableExpression("lorem", StringComparison.OrdinalIgnoreCase));
        private readonly EndsWithCondition EW_SUT = new EndsWithCondition(
            new StringComparableExpression("ipsum", StringComparison.Ordinal));
        private readonly ContainsCondition CC_SUT = new ContainsCondition(
            new StringComparableExpression("m ip", StringComparison.OrdinalIgnoreCase));

        [TestMethod]
        public void SwewTest()
        {
            var reasons = new List<string>();
            Assert.IsTrue(SW_SUT.Evaluate("lorem ipsum", reasons));
            Assert.HasCount(1, reasons);
            Assert.IsTrue(EW_SUT.Evaluate("lorem ipsum", reasons));
            Assert.HasCount(2, reasons);
            Assert.IsTrue(SW_SUT.Evaluate("LOREM IPSUM", reasons));
            Assert.HasCount(3, reasons);
            Assert.IsFalse(EW_SUT.Evaluate("LOREM IPSUM", reasons));
            Assert.HasCount(4, reasons);
            Assert.IsFalse(CC_SUT.Evaluate("loremipsum", reasons));
            Assert.HasCount(5, reasons);
            Assert.IsTrue(CC_SUT.Evaluate("lorem ipsum", reasons));
            Assert.HasCount(6, reasons);
        }
    }
}
