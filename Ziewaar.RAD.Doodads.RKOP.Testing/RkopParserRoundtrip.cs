using System.Xml.Resolvers;

namespace Ziewaar.RAD.Doodads.RKOP.Testing
{
    [TestClass]
    public sealed class RkopParserRoundtrip
    {
        [TestMethod]
        public void TestEmptyFile()
        {
            ServiceDescription desc = new();
            var empty = CursorText.Create(
                new DirectoryInfo(Directory.GetCurrentDirectory()),
                $"");
            var result = desc.UpdateFrom(ref empty);
            Assert.AreEqual(ParityParsingState.Void, result);
        }
        [TestMethod]
        public void TestSpaceFile()
        {
            ServiceDescription desc = new();
            var empty = CursorText.Create(
                new DirectoryInfo(Directory.GetCurrentDirectory()),
                $"""
                   
                """);
            var result = desc.UpdateFrom(ref empty);
            Assert.AreEqual(0, empty.LocalScope.Count);
            Assert.AreEqual(ParityParsingState.Void, result);
        }
        [TestMethod]
        public void TestSimpleDeclaration()
        {
            ServiceDescription desc = new();
            var simple = CursorText.Create(
                new DirectoryInfo(Directory.GetCurrentDirectory()),
                $"""
                Entry->SomeService();
                """);
            var result = desc.UpdateFrom(ref simple);
            Assert.IsTrue(result > ParityParsingState.Unchanged);
            Assert.IsTrue(simple.LocalScope.TryGetValue("Entry", out var cEntry));
            Assert.IsTrue(cEntry is ServiceDescription cDesc && cDesc == desc);
            Assert.IsTrue(simple.LocalScope.TryGetValue("const_Entry", out var consts));
            Assert.IsTrue(consts is ServiceConstantsDescription scDesc && scDesc.Members.Count == 0);
            Assert.AreEqual("Entry", desc.ConstantsDescription.Key);
            Assert.AreEqual("SomeService", desc.ServiceTypeName);
        }


        [TestMethod]
        public void TestConstantsDeclaration()
        {
            ServiceDescription desc = new();
            string cdir = Directory.GetCurrentDirectory();
            var simple = CursorText.Create(
                new DirectoryInfo(cdir),
                $"""
                Entry->SomeService(yotta = "oy!", terra = 123, stinky = False, path = f"hi.txt");
                """);
            var result = desc.UpdateFrom(ref simple);
            Assert.IsTrue(result > ParityParsingState.Unchanged);
            Assert.IsTrue(simple.LocalScope.TryGetValue("Entry", out var cEntry));
            Assert.IsTrue(cEntry is ServiceDescription cDesc && cDesc == desc);
            Assert.IsTrue(simple.LocalScope.TryGetValue("const_Entry", out var consts));
            Assert.IsTrue(consts is ServiceConstantsDescription scDesc && scDesc.Members.Count == 4);
            var assertedConsts = (ServiceConstantsDescription)consts;
            Assert.AreEqual("oy!", assertedConsts.Members.Single(x => x.Key == "yotta").Value.GetValue());
            Assert.AreEqual(123M, assertedConsts.Members.Single(x => x.Key == "terra").Value.GetValue());
            Assert.AreEqual(false, assertedConsts.Members.Single(x => x.Key == "stinky").Value.GetValue());
            (string x, string y) registeredPath = ((string x, string y))assertedConsts.Members.Single(x => x.Key == "path").Value.GetValue();
            Assert.AreEqual(Path.Combine(cdir, "hi.txt"), Path.Combine(registeredPath.x, registeredPath.y));
            Assert.AreEqual("Entry", desc.ConstantsDescription.Key);
            Assert.AreEqual("SomeService", desc.ServiceTypeName);
        }


        [TestMethod]
        public void TestNestingConstantsDeclaration()
        {
            ServiceDescription desc = new();
            string cdir = Directory.GetCurrentDirectory();
            var simple = CursorText.Create(
                new DirectoryInfo(cdir),
                """
                Entry->SomeService(yotta = "oy!", terra = 123, stinky = False, path = f"hi.txt") {
                    Child->OtherService(exa = "beep");
                    Child2->MoreService(peta = "boop");
                };
                """);
            var result = desc.UpdateFrom(ref simple);
            Assert.IsTrue(result > ParityParsingState.Unchanged);
            Assert.IsTrue(simple.LocalScope.TryGetValue("Entry", out var cEntry));
            Assert.IsTrue(cEntry is ServiceDescription cDesc && cDesc == desc);
            Assert.IsTrue(simple.LocalScope.TryGetValue("const_Entry", out var consts));
            Assert.IsTrue(consts is ServiceConstantsDescription scDesc && scDesc.Members.Count == 4);
            var assertedConsts = (ServiceConstantsDescription)consts;
            Assert.AreEqual("oy!", assertedConsts.Members.Single(x => x.Key == "yotta").Value.GetValue());
            Assert.AreEqual(123M, assertedConsts.Members.Single(x => x.Key == "terra").Value.GetValue());
            Assert.AreEqual(false, assertedConsts.Members.Single(x => x.Key == "stinky").Value.GetValue());
            (string x, string y) registeredPath = ((string x, string y))assertedConsts.Members.Single(x => x.Key == "path").Value.GetValue();
            Assert.AreEqual(Path.Combine(cdir, "hi.txt"), Path.Combine(registeredPath.x, registeredPath.y));
            Assert.AreEqual("Entry", desc.ConstantsDescription.Key);
            Assert.AreEqual("SomeService", desc.ServiceTypeName);

            Assert.AreEqual(2, desc.Children.Count);
            Assert.AreEqual("Child", desc.Children[0].ConstantsDescription.Key);
            Assert.AreEqual("Child2", desc.Children[1].ConstantsDescription.Key);
            Assert.AreEqual("OtherService", desc.Children[0].ServiceTypeName);
            Assert.AreEqual("MoreService", desc.Children[1].ServiceTypeName);
        }
        [TestMethod]
        public void TestNestingReferringConstantsDeclaration()
        {
            ServiceDescription desc = new();
            string cdir = Directory.GetCurrentDirectory();
            var simple = CursorText.Create(
                new DirectoryInfo(cdir),
                """
                Entry->SomeService(yotta = "oy!", terra = 123, stinky = False, path = f"hi.txt") {
                    Child->OtherService(exa = "beep");
                    _EarlyDefine->SecretService();
                    Child2->MoreService(peta = "boop") {
                        CallbackBranch->_EarlyDefine;
                    };
                };
                """);
            var result = desc.UpdateFrom(ref simple);
            Assert.IsTrue(result > ParityParsingState.Unchanged);
            Assert.IsTrue(simple.LocalScope.TryGetValue("Entry", out var cEntry));
            Assert.IsTrue(cEntry is ServiceDescription cDesc && cDesc == desc);
            Assert.IsTrue(simple.LocalScope.TryGetValue("const_Entry", out var consts));
            Assert.IsTrue(consts is ServiceConstantsDescription scDesc && scDesc.Members.Count == 4);
            var assertedConsts = (ServiceConstantsDescription)consts;
            Assert.AreEqual("oy!", assertedConsts.Members.Single(x => x.Key == "yotta").Value.GetValue());
            Assert.AreEqual(123M, assertedConsts.Members.Single(x => x.Key == "terra").Value.GetValue());
            Assert.AreEqual(false, assertedConsts.Members.Single(x => x.Key == "stinky").Value.GetValue());
            (string x, string y) registeredPath = ((string x, string y))assertedConsts.Members.Single(x => x.Key == "path").Value.GetValue();
            Assert.AreEqual(Path.Combine(cdir, "hi.txt"), Path.Combine(registeredPath.x, registeredPath.y));
            Assert.AreEqual("Entry", desc.ConstantsDescription.Key);
            Assert.AreEqual("SomeService", desc.ServiceTypeName);

            Assert.AreEqual(3, desc.Children.Count);
            Assert.AreEqual("Child", desc.Children[0].ConstantsDescription.Key);
            Assert.AreEqual("_EarlyDefine", desc.Children[1].ConstantsDescription.Key);
            Assert.AreEqual("Child2", desc.Children[2].ConstantsDescription.Key);
            Assert.AreEqual("OtherService", desc.Children[0].ServiceTypeName);
            Assert.AreEqual("SecretService", desc.Children[2].Children[0].RedirectsTo.ServiceTypeName);
            Assert.AreEqual("MoreService", desc.Children[2].ServiceTypeName);
        }
        [TestMethod]
        public void TestRoundtrip()
        {
            var testText = """
                Entry->SomeService(yotta = "oy!", terra = 123, stinky = False, path = f"hi.txt") {
                    Child->OtherService(exa = "beep");
                    _EarlyDefine->SecretService();
                    Child2->MoreService(peta = "boop") {
                        CallbackBranch->_EarlyDefine;
                    };
                };
                """;

            ServiceDescription desc = new();
            string cdir = Directory.GetCurrentDirectory();
            var simple = CursorText.Create(
                new DirectoryInfo(cdir),
                testText);
            var result = desc.UpdateFrom(ref simple);

            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            desc.WriteTo(writer);
            writer.Flush();
            ms.Position = 0;
            var reader = new StreamReader(ms);
            var fullText = reader.ReadToEnd();

            Assert.AreEqual(testText.Trim(), fullText.Trim());
        }

    }
}
