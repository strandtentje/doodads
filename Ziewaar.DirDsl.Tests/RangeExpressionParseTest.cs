using Ziewaar.RAD.Doodads.AdvancedFilesystem.DirDsl;

namespace Ziewaar.DirDsl.Tests
{
    [TestClass]
    public class RangeExpressionParseTest
    {
        private const string EZ_EXP = "_`-123`,`123`";
        private const string AB_EXP = "_'cde','ghi'";

        [TestMethod]
        public void TestNumeric()
        {
            var errors = new List<(int c, string d)>();
            int cursor = 0;
            Assert.IsTrue(RangeCondition.TryParseFrom(EZ_EXP, errors, ref cursor, out var exp));
            Assert.IsEmpty(errors);
            Assert.AreEqual(EZ_EXP.Length, cursor);
            var reasons = new List<string>();
            Assert.IsFalse(exp.Evaluate("-200", reasons));
            Assert.HasCount(1, reasons);
            Assert.IsTrue(exp.Evaluate("-100", reasons));
            Assert.HasCount(2, reasons);
            Assert.IsTrue(exp.Evaluate("0", reasons));
            Assert.HasCount(3, reasons);
            Assert.IsTrue(exp.Evaluate("100", reasons));
            Assert.HasCount(4, reasons);
            Assert.IsFalse(exp.Evaluate("200", reasons));
            Assert.HasCount(5, reasons);
            Assert.IsFalse(exp.Evaluate("abc", reasons));
            Assert.HasCount(6, reasons);
        }

        [TestMethod]
        public void TestAlpha()
        {
            var errors = new List<(int c, string d)>();
            int cursor = 0;
            Assert.IsTrue(RangeCondition.TryParseFrom(AB_EXP, errors, ref cursor, out var exp));
            Assert.IsEmpty(errors);
            Assert.AreEqual(AB_EXP.Length, cursor);
            var reasons = new List<string>();
            Assert.IsFalse(exp.Evaluate("abc", reasons));
            Assert.HasCount(1, reasons);
            Assert.IsTrue(exp.Evaluate("cde", reasons));
            Assert.HasCount(2, reasons);
            Assert.IsTrue(exp.Evaluate("def", reasons));
            Assert.HasCount(3, reasons);
            Assert.IsTrue(exp.Evaluate("efg", reasons));
            Assert.HasCount(4, reasons);
            Assert.IsFalse(exp.Evaluate("ghi", reasons));
            Assert.HasCount(5, reasons);
            Assert.IsFalse(exp.Evaluate("jkl", reasons));
            Assert.HasCount(6, reasons);
            Assert.IsFalse(exp.Evaluate("123", reasons));
            Assert.HasCount(7, reasons);

        }
    }

}
