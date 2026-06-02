using Ziewaar.RAD.Doodads.AdvancedFilesystem.DirDsl;

namespace Ziewaar.DirDsl.Tests
{
    [TestClass]
    public class AndBooleanSeriesTest
    {
        private readonly AndBooleanSeries SUT = new AndBooleanSeries([
            new ContainsCondition(new StringComparableExpression("ore", StringComparison.OrdinalIgnoreCase)),
            new ContainsCondition(new StringComparableExpression("psu", StringComparison.OrdinalIgnoreCase))]);

        [TestMethod]
        public void Test()
        {
            var reasons = new List<string>();
            Assert.IsFalse(SUT.Evaluate("lorem", reasons));
            Assert.IsFalse(SUT.Evaluate("IPSUM", reasons));
            Assert.IsTrue(SUT.Evaluate("lorem ipsum", reasons));
            Assert.IsFalse(SUT.Evaluate("", reasons));
            Assert.IsFalse(SUT.Evaluate("banana", reasons));            
        }
    }
}
