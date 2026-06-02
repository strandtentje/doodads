using Ziewaar.RAD.Doodads.AdvancedFilesystem.DirDsl;

namespace Ziewaar.DirDsl.Tests
{
    [TestClass]
    public class EqualsBooleanSeriesTest
    {
        private readonly AllSameBooleanSeries SUT = new AllSameBooleanSeries([
            new ContainsCondition(new StringComparableExpression("ore", StringComparison.OrdinalIgnoreCase)),
            new ContainsCondition(new StringComparableExpression("psu", StringComparison.OrdinalIgnoreCase))]);

        [TestMethod]
        public void Test()
        {
            var reasons = new List<string>();
            Assert.IsFalse(SUT.Evaluate("lorem", reasons));
            Assert.HasCount(3, reasons);
            Assert.IsFalse(SUT.Evaluate("IPSUM", reasons));
            Assert.HasCount(6, reasons);
            Assert.IsTrue(SUT.Evaluate("lorem ipsum", reasons));
            Assert.HasCount(9, reasons);
            Assert.IsTrue(SUT.Evaluate("", reasons));            
        }
    }
}
