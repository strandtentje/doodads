using Ziewaar.RAD.Doodads.AdvancedFilesystem.DirDsl;

namespace Ziewaar.DirDsl.Tests
{
    [TestClass]
    public class OrBooleanSeriesTest
    {
        private readonly OrBooleanSeries SUT = new OrBooleanSeries([
            new ContainsCondition(new StringComparableExpression("ore", StringComparison.OrdinalIgnoreCase)),
            new ContainsCondition(new StringComparableExpression("psu", StringComparison.OrdinalIgnoreCase))]);

        [TestMethod]
        public void Test()
        {
            var reasons = new List<string>();
            Assert.IsTrue(SUT.Evaluate("lorem", reasons));
            Assert.IsTrue(SUT.Evaluate("IPSUM", reasons));
            Assert.IsTrue(SUT.Evaluate("lorem ipsum", reasons));
            Assert.IsFalse(SUT.Evaluate("", reasons));
            Assert.IsFalse(SUT.Evaluate("banana", reasons));            
        }
    }
}
