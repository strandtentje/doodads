[assembly: Parallelize(Scope = ExecutionScope.MethodLevel)]
namespace Ziewaar.Common.Aardvargs.Tests
{
    [TestClass]
    public class AardvargsTests
    {
        [TestMethod]
        public void Parses_Positional_Arguments_As_Filenames()
        {
            var args = new[] { "file1.txt", "file2.txt" };
            var parsed = ArgParser.Parse(args);
            CollectionAssert.AreEqual(new List<string> { "file1.txt", "file2.txt" }, parsed.Filenames);
        }

        [TestMethod]
        public void Parses_Short_Flags_As_Bools()
        {
            var args = new[] { "-v", "-x" };
            var parsed = ArgParser.Parse(args);
            Assert.IsTrue((bool?)parsed.Options["v"]);
            Assert.IsTrue((bool?)parsed.Options["x"]);
        }

        [TestMethod]
        public void Parses_Combined_Short_Flags()
        {
            var args = new[] { "-abc" };
            var parsed = ArgParser.Parse(args);
            Assert.IsTrue((bool?)parsed.Options["a"]);
            Assert.IsTrue((bool?)parsed.Options["b"]);
            Assert.IsTrue((bool?)parsed.Options["c"]);
        }

        [TestMethod]
        public void Parses_Short_Option_With_Value()
        {
            var args = new[] { "-o", "output.txt" };
            var parsed = ArgParser.Parse(args);
            Assert.AreEqual("output.txt", parsed.Options["o"]);
        }

        [TestMethod]
        public void Parses_Short_Option_With_Equal_Sign()
        {
            var args = new[] { "-o=output.txt" };
            var parsed = ArgParser.Parse(args);
            Assert.AreEqual("output.txt", parsed.Options["o"]);
        }

        [TestMethod]
        public void Parses_Long_Option_With_Equal_Sign()
        {
            var args = new[] { "--threads=4" };
            var parsed = ArgParser.Parse(args);
            Assert.AreEqual(4, parsed.Options.Get<int>("threads"));
        }

        [TestMethod]
        public void Parses_Long_Option_With_Space_Separated_Value()
        {
            var args = new[] { "--output", "result.txt" };
            var parsed = ArgParser.Parse(args);
            Assert.AreEqual("result.txt", parsed.Options["output"]);
        }

        [TestMethod]
        public void Coerces_Boolean_And_Numeric_Values()
        {
            var args = new[] { "--debug=True", "--max=42", "--scale", "3.14" };
            var parsed = ArgParser.Parse(args);
            Assert.IsTrue(parsed.Options.Get<bool>("debug"));
            Assert.AreEqual(42, parsed.Options.Get<int>("max"));
            Assert.AreEqual(3.14m, parsed.Options.Get<decimal>("scale"));
        }

        [TestMethod]
        public void Handles_Quotes_Around_Values()
        {
            var args = new[] { "--path", "\"C:\\Program Files\\App\"" };
            var parsed = ArgParser.Parse(args);
            Assert.AreEqual("C:\\Program Files\\App", parsed.Options["path"]);
        }

        [TestMethod]
        public void Stops_Parsing_Options_After_DoubleDash()
        {
            var args = new[] { "--threads=2", "--", "--not-an-option", "positional" };
            var parsed = ArgParser.Parse(args);
            Assert.AreEqual(2, parsed.Options.Get<int>("threads"));
            CollectionAssert.Contains(parsed.Filenames, "--not-an-option");
            CollectionAssert.Contains(parsed.Filenames, "positional");
        }

        [TestMethod]
        public void Missing_Option_Value_Does_Not_Throw()
        {
            var args = new[] { "--flag" };
            var parsed = ArgParser.Parse(args);
            Assert.IsTrue((bool?)parsed.Options["flag"]);
        }

        [TestMethod]
        public void Unknown_Key_Returns_Default()
        {
            var parsed = ArgParser.Parse(new string[0]);
            var value = parsed.Options.Get<int>("missing", 99);
            Assert.AreEqual(99, value);
        }
    }
}
