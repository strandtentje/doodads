using Ziewaar.RAD.Doodads.AdvancedFilesystem.DirDsl;

namespace Ziewaar.DirDsl.Tests
{
    [TestClass]
    public class EqualsConditionEvalTest
    {
        private readonly EqualsCondition AB_SUT = new EqualsCondition(new StringComparableExpression("def", StringComparison.OrdinalIgnoreCase));
        private readonly EqualsCondition NM_SUT = new EqualsCondition(new ComparableExpression(234));

        [TestMethod]
        public void TestAlpha()
        {
            var reasons = new List<string>();
            Assert.IsFalse(AB_SUT.Evaluate("abc", reasons));
            Assert.HasCount(1, reasons);
            Assert.IsTrue(AB_SUT.Evaluate("def", reasons));
            Assert.HasCount(2, reasons);
            Assert.IsTrue(AB_SUT.Evaluate("DEF", reasons));
            Assert.HasCount(3, reasons);
            Assert.IsFalse(AB_SUT.Evaluate("ghi", reasons));
            Assert.HasCount(4, reasons);
        }

        [TestMethod]
        public void TestNumeric()
        {
            var reasons = new List<string>();
            Assert.IsFalse(NM_SUT.Evaluate("123", reasons));
            Assert.HasCount(1, reasons);
            Assert.IsTrue(NM_SUT.Evaluate("234", reasons));
            Assert.HasCount(2, reasons);
            Assert.IsFalse(NM_SUT.Evaluate("456", reasons));
            Assert.HasCount(3, reasons);
            Assert.IsFalse(NM_SUT.Evaluate("ghi", reasons));
            Assert.HasCount(4, reasons);
        }
    }


}
