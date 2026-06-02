using Ziewaar.RAD.Doodads.AdvancedFilesystem.DirDsl;

namespace Ziewaar.DirDsl.Tests
{
    [TestClass]
    public class ComparingConditionParseTest
    {
        private readonly string
            NUM_RANGE = "(_`1`,`10`)",
            ABC_RANGE = "(_'abc','ghi')",
            NUM_ABOVE = "+`123`",
            ABC_ABOVE = "+'abc'",
            NUM_BELOW = "-`123`",
            ABC_BELOW = "-'abc'",
            NUM_INVRT = "~`123`",
            ABC_INVRT = "~'abc'",
            NUM_EQUAL = "`123`",
            ABC_EQUAL = "'abc'",
            ABC_START = "$'abc'",
            ABC_TRAIL = ";'abc'%",
            ABC_CONTS = "ß'abc'";

        [TestMethod]
        public void TestNumRange()
        {
            var errors = new List<(int c, string d)>();
            int cursor = 0;
            Assert.IsTrue(ComparingCondition.TryParseFrom(NUM_RANGE, errors, ref cursor, out var exp));
            Assert.IsEmpty(errors);
            var c = Assert.IsInstanceOfType<RangeCondition>(exp);
            var l = Assert.IsInstanceOfType<ComparableExpression>(c.LBound);
            Assert.AreEqual("numeric [1]", l.ToString());
            var u = Assert.IsInstanceOfType<ComparableExpression>(c.UBound);
            Assert.AreEqual("numeric [10]", u.ToString());
        }
        [TestMethod]
        public void TestAlphaRange()
        {
            var errors = new List<(int c, string d)>();
            int cursor = 0;
            Assert.IsTrue(ComparingCondition.TryParseFrom(ABC_RANGE, errors, ref cursor, out var exp));
            Assert.IsEmpty(errors);
            var c = Assert.IsInstanceOfType<RangeCondition>(exp);
            var l = Assert.IsInstanceOfType<StringComparableExpression>(c.LBound);
            Assert.AreEqual("textual abc", l.ToString());
            var u = Assert.IsInstanceOfType<StringComparableExpression>(c.UBound);
            Assert.AreEqual("textual ghi", u.ToString());
        }
        [TestMethod]
        public void TestNumAbove()
        {
            var errors = new List<(int c, string d)>();
            int cursor = 0;
            Assert.IsTrue(ComparingCondition.TryParseFrom(NUM_ABOVE, errors, ref cursor, out var exp));
            Assert.IsEmpty(errors);
            var c = Assert.IsInstanceOfType<GreaterInclusiveCondition>(exp);
            var l = Assert.IsInstanceOfType<ComparableExpression>(c.Operand);
            Assert.AreEqual("numeric [123]", l.ToString());
        }

        [TestMethod]
        public void TestAbcAbove()
        {
            var errors = new List<(int c, string d)>();
            int cursor = 0;
            Assert.IsTrue(ComparingCondition.TryParseFrom(ABC_ABOVE, errors, ref cursor, out var exp));
            Assert.IsEmpty(errors);
            var c = Assert.IsInstanceOfType<GreaterInclusiveCondition>(exp);
            var l = Assert.IsInstanceOfType<StringComparableExpression>(c.Operand);
            Assert.AreEqual("textual abc", l.ToString());
        }
        [TestMethod]
        public void TestNumBelow()
        {
            var errors = new List<(int c, string d)>();
            int cursor = 0;
            Assert.IsTrue(ComparingCondition.TryParseFrom(NUM_BELOW, errors, ref cursor, out var exp));
            Assert.IsEmpty(errors);
            var c = Assert.IsInstanceOfType<LesserExclusiveCondition>(exp);
            var l = Assert.IsInstanceOfType<ComparableExpression>(c.Operand);
            Assert.AreEqual("numeric [123]", l.ToString());
        }
        [TestMethod]
        public void TestAbcBelow()
        {
            var errors = new List<(int c, string d)>();
            int cursor = 0;
            Assert.IsTrue(ComparingCondition.TryParseFrom(ABC_BELOW, errors, ref cursor, out var exp));
            Assert.IsEmpty(errors);
            var c = Assert.IsInstanceOfType<LesserExclusiveCondition>(exp);
            var l = Assert.IsInstanceOfType<StringComparableExpression>(c.Operand);
            Assert.AreEqual("textual abc", l.ToString());
        }
        [TestMethod]
        public void TestNumEqual()
        {
            var errors = new List<(int c, string d)>();
            int cursor = 0;
            Assert.IsTrue(ComparingCondition.TryParseFrom(NUM_EQUAL, errors, ref cursor, out var exp));
            Assert.IsEmpty(errors);
            var c = Assert.IsInstanceOfType<EqualsCondition>(exp);
            var l = Assert.IsInstanceOfType<ComparableExpression>(c.Operand);
            Assert.AreEqual("numeric [123]", l.ToString());
        }
        [TestMethod]
        public void TestAbcEqual()
        {
            var errors = new List<(int c, string d)>();
            int cursor = 0;
            Assert.IsTrue(ComparingCondition.TryParseFrom(ABC_EQUAL, errors, ref cursor, out var exp));
            Assert.IsEmpty(errors);
            var c = Assert.IsInstanceOfType<EqualsCondition>(exp);
            var l = Assert.IsInstanceOfType<StringComparableExpression>(c.Operand);
            Assert.AreEqual("textual abc", l.ToString());
        }
        [TestMethod]
        public void TestNumNotEqual()
        {
            var errors = new List<(int c, string d)>();
            int cursor = 0;
            Assert.IsTrue(ComparingCondition.TryParseFrom(NUM_INVRT, errors, ref cursor, out var exp));
            Assert.IsEmpty(errors);
            var c = Assert.IsInstanceOfType<NotEqualsCondition>(exp);
            var l = Assert.IsInstanceOfType<ComparableExpression>(c.Operand);
            Assert.AreEqual("numeric [123]", l.ToString());
        }
        [TestMethod]
        public void TestAbcNotEqual()
        {
            var errors = new List<(int c, string d)>();
            int cursor = 0;
            Assert.IsTrue(ComparingCondition.TryParseFrom(ABC_INVRT, errors, ref cursor, out var exp));
            Assert.IsEmpty(errors);
            var c = Assert.IsInstanceOfType<NotEqualsCondition>(exp);
            var l = Assert.IsInstanceOfType<StringComparableExpression>(c.Operand);
            Assert.AreEqual("textual abc", l.ToString());
        }
        [TestMethod]
        public void TestAbcStart()
        {
            var errors = new List<(int c, string d)>();
            int cursor = 0;
            Assert.IsTrue(ComparingCondition.TryParseFrom(ABC_START, errors, ref cursor, out var exp));
            Assert.IsEmpty(errors);
            var c = Assert.IsInstanceOfType<StartsWithCondition>(exp);
            var l = Assert.IsInstanceOfType<StringComparableExpression>(c.Operand);
            Assert.AreEqual("textual abc", l.ToString());
        }
        [TestMethod]
        public void TestAbcEnd()
        {
            var errors = new List<(int c, string d)>();
            int cursor = 0;
            Assert.IsTrue(ComparingCondition.TryParseFrom(ABC_TRAIL, errors, ref cursor, out var exp));
            Assert.IsEmpty(errors);
            var c = Assert.IsInstanceOfType<EndsWithCondition>(exp);
            var l = Assert.IsInstanceOfType<StringComparableExpression>(c.Operand);
            Assert.AreEqual("textual abc", l.ToString());
        }
        [TestMethod]
        public void TestAbcCont()
        {
            var errors = new List<(int c, string d)>();
            int cursor = 0;
            Assert.IsTrue(ComparingCondition.TryParseFrom(ABC_CONTS, errors, ref cursor, out var exp));
            Assert.IsEmpty(errors);
            var c = Assert.IsInstanceOfType<ContainsCondition>(exp);
            var l = Assert.IsInstanceOfType<StringComparableExpression>(c.Operand);
            Assert.AreEqual("textual abc", l.ToString());
        }
    }
}
