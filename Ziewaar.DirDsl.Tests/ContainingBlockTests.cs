using Ziewaar.RAD.Doodads.AdvancedFilesystem.DirDsl;

namespace Ziewaar.DirDsl.Tests
{
    [TestClass]
    public class ContainingBlockTests
    {
        private readonly string PLAIN = "plain", SINGLE = "#~'cde'", FULL = "#[.ß'abc',ß'def']";
        [TestMethod]
        public void TestMain()
        {
            var plainEx = ContainingBlock.ParseFrom(PLAIN, out var err);
            var reasons = new List<string>();
            Assert.IsTrue(plainEx.Evaluate("plain", reasons));
        }
        [TestMethod]
        public void TestSingle()
        {
            var plainEx = ContainingBlock.ParseFrom(SINGLE, out var err);
            var reasons = new List<string>();
            Assert.IsTrue(plainEx.Evaluate("def", reasons));
            Assert.IsFalse(plainEx.Evaluate("cde", reasons));
        }
        [TestMethod]
        public void TestFull()
        {
            var plainEx = ContainingBlock.ParseFrom(FULL, out var err);
            var reasons = new List<string>();
            Assert.IsTrue(plainEx.Evaluate("def", reasons));
            Assert.IsFalse(plainEx.Evaluate("cde", reasons));
            Assert.IsTrue(plainEx.Evaluate("zyxabced", reasons));
        }
    }
}
