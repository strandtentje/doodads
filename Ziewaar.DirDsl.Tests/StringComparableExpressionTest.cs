using Microsoft.VisualStudio.TestPlatform.ObjectModel.Utilities;
using Ziewaar.RAD.Doodads.AdvancedFilesystem.DirDsl;

namespace Ziewaar.DirDsl.Tests
{
    [TestClass]
    public sealed class StringComparableExpressionTest
    {
        private readonly StringComparableExpression SUT_CI = new StringComparableExpression("LOREM", StringComparison.OrdinalIgnoreCase);
        private readonly StringComparableExpression SUT_CS = new StringComparableExpression("ipsum", StringComparison.Ordinal);
        [TestMethod]
        public void TestCI()
        {
            Assert.AreEqual("LOREM", SUT_CI.Literal);
            Assert.AreEqual("textual LOREM", SUT_CI.ToString());
            Assert.IsTrue(SUT_CI.TryCompare("ipsum", out var shouldBeLessBecauseIbeforeL));
            Assert.IsLessThan(0, shouldBeLessBecauseIbeforeL);
            Assert.IsTrue(SUT_CI.TryCompare("APPLE", out var shouldBeLessBecauseAbeforeL));
            Assert.IsLessThan(0, shouldBeLessBecauseAbeforeL);
            Assert.IsTrue(SUT_CI.TryCompare("LOREM", out var shouldBeEqual));
            Assert.AreEqual(0, shouldBeEqual);
            Assert.IsTrue(SUT_CI.TryCompare("lorem", out var shouldBeEqualBecauseCI));
            Assert.AreEqual(0, shouldBeEqualBecauseCI);
            Assert.IsTrue(SUT_CS.TryCompare("zorro", out var shouldBeGreaterBecauseZafterL));
            Assert.IsGreaterThan(0, shouldBeGreaterBecauseZafterL);
        }

        [TestMethod]
        public void TestCS()
        {
            Assert.AreEqual("ipsum", SUT_CS.Literal);
            Assert.AreEqual("textual ipsum", SUT_CS.ToString());
            Assert.IsTrue(SUT_CS.TryCompare("LOREM", out var shouldBeLessBecauseCapsBeforeLower));
            Assert.IsLessThan(0, shouldBeLessBecauseCapsBeforeLower);
            Assert.IsTrue(SUT_CS.TryCompare("lorem", out var shouldBeGreaterBecauseLafterI));
            Assert.IsGreaterThan(0, shouldBeGreaterBecauseLafterI);
            Assert.IsTrue(SUT_CS.TryCompare("zorro", out var shouldBeGreaterBecauseZafterI));
            Assert.IsGreaterThan(0, shouldBeGreaterBecauseZafterI);
            Assert.IsTrue(SUT_CS.TryCompare("ipsum", out var shouldBeSame));
            Assert.AreEqual(0, shouldBeSame);
            Assert.IsTrue(SUT_CS.TryCompare("IPSUM", out var shouldBeLessBecauseCapsBeforeLower2));
            Assert.IsLessThan(0, shouldBeLessBecauseCapsBeforeLower2);
        }
    }
}
