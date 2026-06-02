using Ziewaar.RAD.Doodads.AdvancedFilesystem.DirDsl;

namespace Ziewaar.DirDsl.Tests
{
    [TestClass]
    public class XorBooleanSeriesTest
    {
        private readonly XorBooleanSeries SUT = new XorBooleanSeries([
            new ContainsCondition(new StringComparableExpression("ore", StringComparison.OrdinalIgnoreCase)),
            new ContainsCondition(new StringComparableExpression("psu", StringComparison.OrdinalIgnoreCase))]);

        [TestMethod]
        public void Test()
        {
            var reasons = new List<string>();
            Assert.IsTrue(SUT.Evaluate("lorem", reasons));
            Assert.HasCount(5, reasons);
            Assert.IsTrue(SUT.Evaluate("IPSUM", reasons));
            Assert.HasCount(10, reasons);
            Assert.IsFalse(SUT.Evaluate("lorem ipsum", reasons));
            Assert.HasCount(15, reasons);
        }
    }
}
