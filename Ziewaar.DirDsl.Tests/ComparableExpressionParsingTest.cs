using Ziewaar.RAD.Doodads.AdvancedFilesystem.DirDsl;

namespace Ziewaar.DirDsl.Tests
{
    [TestClass]
    public class ComparableExpressionParsingTest
    {
        [TestMethod]
        public void TestPositiveNumeric()
        {
            var errors = new List<(int c, string d)>();
            int cursor = 0;
            Assert.IsTrue(ComparableExpression.TryParseFrom("`123`", errors, ref cursor, false, out var exp));
            Assert.AreEqual(5, cursor);
            Assert.IsEmpty(errors);
            Assert.AreEqual("numeric [123]", exp.ToString());
            Assert.AreEqual("123", exp.Literal);
            Assert.IsTrue(exp.TryCompare("123", out int rel));
            Assert.AreEqual(0, rel);
        }
        [TestMethod]
        public void TestNegativeNumeric()
        {
            var errors = new List<(int c, string d)>();
            int cursor = 0;
            Assert.IsTrue(ComparableExpression.TryParseFrom("`-123`", errors, ref cursor, false, out var exp));
            Assert.AreEqual(6, cursor);
            Assert.IsEmpty(errors);
            Assert.AreEqual("numeric [-123]", exp.ToString());
            Assert.AreEqual("-123", exp.Literal);
            Assert.IsTrue(exp.TryCompare("-123", out int rel));
            Assert.AreEqual(0, rel);
        }
        [TestMethod]
        public void TestZeroNumeric()
        {
            var errors = new List<(int c, string d)>();
            int cursor = 0;
            Assert.IsTrue(ComparableExpression.TryParseFrom("`0`", errors, ref cursor, false, out var exp));
            Assert.AreEqual(3, cursor);
            Assert.IsEmpty(errors);
            Assert.AreEqual("numeric [0]", exp.ToString());
            Assert.AreEqual("0", exp.Literal);
            Assert.IsTrue(exp.TryCompare("0", out int rel));
            Assert.AreEqual(0, rel);
        }
        [TestMethod]
        public void TestIllegalNumeric1()
        {
            var errors = new List<(int c, string d)>();
            int cursor = 0;
            Assert.IsFalse(ComparableExpression.TryParseFrom("`1-1`", errors, ref cursor, false, out var exp));
            Assert.AreEqual(5, cursor);
            Assert.IsNotEmpty(errors);
        }
        [TestMethod]
        public void TestIllegalNumeric2()
        {
            var errors = new List<(int c, string d)>();
            int cursor = 0;
            Assert.IsFalse(ComparableExpression.TryParseFrom("`abc`", errors, ref cursor, false, out var exp));
            Assert.AreEqual(1, cursor);
            Assert.IsNotEmpty(errors);
        }
        [TestMethod]
        public void TestCiString()
        {
            var errors = new List<(int c, string d)>();
            int cursor = 0;
            Assert.IsTrue(ComparableExpression.TryParseFrom("'abc'", errors, ref cursor, false, out var exp));
            Assert.AreEqual(5, cursor);
            Assert.IsEmpty(errors);
            Assert.AreEqual("textual abc", exp.ToString());
            Assert.AreEqual("abc", exp.Literal);
            Assert.IsTrue(exp.TryCompare("ABC", out int rel));
            Assert.AreEqual(0, rel);
        }
        [TestMethod]
        public void TestCsString()
        {
            var errors = new List<(int c, string d)>();
            int cursor = 0;
            Assert.IsTrue(ComparableExpression.TryParseFrom("'abc'%", errors, ref cursor, false, out var exp));
            Assert.AreEqual(6, cursor);
            Assert.IsEmpty(errors);
            Assert.AreEqual("textual abc", exp.ToString());
            Assert.AreEqual("abc", exp.Literal);
            Assert.IsTrue(exp.TryCompare("abc", out int rel));
            Assert.AreEqual(0, rel);
            Assert.IsTrue(exp.TryCompare("ABC", out int rel2));
            Assert.IsLessThan(0, rel2);
        }
    }
}
