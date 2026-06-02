using Ziewaar.RAD.Doodads.AdvancedFilesystem.DirDsl;

namespace Ziewaar.DirDsl.Tests
{
    [TestClass]
    public class RangeConditionEvalTest
    {
        private readonly RangeCondition SUT_BZ = new RangeCondition('_', new ComparableExpression(-456), new ComparableExpression(-123));
        private readonly RangeCondition SUT_EZ = new RangeCondition('_', new ComparableExpression(-123), new ComparableExpression(123));
        private readonly RangeCondition SUT_AZ = new RangeCondition('_', new ComparableExpression(123), new ComparableExpression(456));

        [TestMethod]
        public void TestBZ()
        {
            List<string> reasons = new List<string>();
            Assert.IsFalse(SUT_BZ.Evaluate("-800", reasons));
            Assert.HasCount(1, reasons);
            Assert.IsTrue(SUT_BZ.Evaluate("-400", reasons));
            Assert.HasCount(2, reasons);
            Assert.IsFalse(SUT_BZ.Evaluate("-000", reasons));
            Assert.HasCount(3, reasons);
            Assert.IsFalse(SUT_BZ.Evaluate("200", reasons));
            Assert.HasCount(4, reasons);
            Assert.IsFalse(SUT_BZ.Evaluate("400", reasons));
            Assert.HasCount(5, reasons);
            Assert.IsFalse(SUT_BZ.Evaluate("600", reasons));
            Assert.HasCount(6, reasons);
            Assert.IsFalse(SUT_BZ.Evaluate("abc", reasons));
            Assert.HasCount(7, reasons);
        }
        [TestMethod]
        public void TestEZ()
        {
            List<string> reasons = new List<string>();
            Assert.IsFalse(SUT_EZ.Evaluate("-800", reasons));
            Assert.HasCount(1, reasons);
            Assert.IsFalse(SUT_EZ.Evaluate("-400", reasons));
            Assert.HasCount(2, reasons);
            Assert.IsTrue(SUT_EZ.Evaluate("-000", reasons));
            Assert.HasCount(3, reasons);
            Assert.IsTrue(SUT_EZ.Evaluate("100", reasons));
            Assert.HasCount(4, reasons);
            Assert.IsFalse(SUT_EZ.Evaluate("400", reasons));
            Assert.HasCount(5, reasons);
            Assert.IsFalse(SUT_EZ.Evaluate("600", reasons));
            Assert.HasCount(6, reasons);
            Assert.IsFalse(SUT_EZ.Evaluate("abc", reasons));
            Assert.HasCount(7, reasons);
        }

        [TestMethod]
        public void TestAZ()
        {
            List<string> reasons = new List<string>();
            Assert.IsFalse(SUT_AZ.Evaluate("-800", reasons));
            Assert.HasCount(1, reasons);
            Assert.IsFalse(SUT_AZ.Evaluate("-400", reasons));
            Assert.HasCount(2, reasons);
            Assert.IsFalse(SUT_AZ.Evaluate("-000", reasons));
            Assert.HasCount(3, reasons);
            Assert.IsTrue(SUT_AZ.Evaluate("200", reasons));
            Assert.HasCount(4, reasons);
            Assert.IsTrue(SUT_AZ.Evaluate("400", reasons));
            Assert.HasCount(5, reasons);
            Assert.IsFalse(SUT_AZ.Evaluate("600", reasons));
            Assert.HasCount(6, reasons);
            Assert.IsFalse(SUT_AZ.Evaluate("abc", reasons));
            Assert.HasCount(7, reasons);
        }
    }

}
