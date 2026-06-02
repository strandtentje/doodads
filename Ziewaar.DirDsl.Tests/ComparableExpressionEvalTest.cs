using Ziewaar.RAD.Doodads.AdvancedFilesystem.DirDsl;

namespace Ziewaar.DirDsl.Tests
{
    [TestClass]
    public class ComparableExpressionEvalTest
    {
        private readonly ComparableExpression SUT_POS = new ComparableExpression(123);
        private readonly ComparableExpression SUT_ZERO = new ComparableExpression(0);
        private readonly ComparableExpression SUT_NEG = new ComparableExpression(-123);
        [TestMethod]
        public void TestPos()
        {
            Assert.AreEqual("123", SUT_POS.Literal);
            Assert.AreEqual("numeric [123]", SUT_POS.ToString());
            Assert.IsFalse(SUT_POS.TryCompare("abc", out int _));
            Assert.IsFalse(SUT_POS.TryCompare("ab0c", out int _));
            Assert.IsTrue(SUT_POS.TryCompare("123", out int shouldBeSame));
            Assert.AreEqual(0, shouldBeSame);
            Assert.IsTrue(SUT_POS.TryCompare("456", out int shouldBeMore));
            Assert.IsGreaterThan(0, shouldBeMore);
            Assert.IsTrue(SUT_POS.TryCompare("012", out int shouldBeLess));
            Assert.IsLessThan(0, shouldBeLess);
            Assert.IsTrue(SUT_POS.TryCompare("-12", out int shouldBeEvenLess));
            Assert.IsLessThan(0, shouldBeEvenLess);
        }

        [TestMethod]
        public void TestZero()
        {
            Assert.AreEqual("0", SUT_ZERO.Literal);
            Assert.AreEqual("numeric [0]", SUT_ZERO.ToString());
            Assert.IsFalse(SUT_ZERO.TryCompare("abc", out int _));
            Assert.IsFalse(SUT_ZERO.TryCompare("ab0c", out int _));
            Assert.IsTrue(SUT_ZERO.TryCompare("123", out int shouldBeWayMore));
            Assert.IsGreaterThan(0, shouldBeWayMore);
            Assert.IsTrue(SUT_ZERO.TryCompare("456", out int shouldBeLotsMore));
            Assert.IsGreaterThan(0, shouldBeLotsMore);
            Assert.IsTrue(SUT_ZERO.TryCompare("012", out int shouldBeMore));
            Assert.IsGreaterThan(0, shouldBeMore);

            Assert.IsTrue(SUT_ZERO.TryCompare("0", out int shouldBeSame1));
            Assert.AreEqual(0, shouldBeSame1);
            Assert.IsTrue(SUT_ZERO.TryCompare("00", out int shouldBeSame2));
            Assert.AreEqual(0, shouldBeSame2);
            Assert.IsTrue(SUT_ZERO.TryCompare("-0", out int shouldBeSame3));
            Assert.AreEqual(0, shouldBeSame3);

            Assert.IsTrue(SUT_ZERO.TryCompare("-12", out int shouldBeLess));
            Assert.IsLessThan(0, shouldBeLess);
        }

        [TestMethod]
        public void TestNeg()
        {
            Assert.AreEqual("-123", SUT_NEG.Literal);
            Assert.AreEqual("numeric [-123]", SUT_NEG.ToString());
            Assert.IsFalse(SUT_NEG.TryCompare("abc", out int _));
            Assert.IsFalse(SUT_NEG.TryCompare("ab0c", out int _));

            Assert.IsTrue(SUT_NEG.TryCompare("123", out int shouldBeLotsMore));
            Assert.IsGreaterThan(0, shouldBeLotsMore);
            Assert.IsTrue(SUT_NEG.TryCompare("456", out int shouldBeWayMore));
            Assert.IsGreaterThan(0, shouldBeWayMore);
            Assert.IsTrue(SUT_NEG.TryCompare("012", out int shouldBeMore));
            Assert.IsGreaterThan(0, shouldBeMore);
            Assert.IsTrue(SUT_NEG.TryCompare("-12", out int shouldStillBeMore));
            Assert.IsGreaterThan(0, shouldStillBeMore);
            Assert.IsTrue(SUT_NEG.TryCompare("-123", out int shouldBeSame));
            Assert.AreEqual(0, shouldBeSame);
            Assert.IsTrue(SUT_NEG.TryCompare("-456", out int shouldBeLess));
            Assert.IsLessThan(0, shouldBeLess);
        }
    }
}
