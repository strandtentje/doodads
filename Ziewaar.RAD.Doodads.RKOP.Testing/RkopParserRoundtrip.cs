using System.Xml.Resolvers;

namespace Ziewaar.RAD.Doodads.RKOP.Testing
{
    [TestClass]
    public sealed class RkopParserRoundtrip
    {
        public class MockWrapper : IInstanceWrapper
        {
            public void Cleanup()
            {

            }

            public void SetDefinition<TResult>(CursorText atPosition, string typename, TResult? nextInLine, TResult? continuation, SortedList<string, object> constants, SortedList<string, TResult> wrappers) where TResult : IInstanceWrapper, new()
            {
                
            }

            void IInstanceWrapper.SetReference<TResult>(ServiceDescription<TResult> redirectsTo)
            {
                
            }
        }
        [TestMethod]
        public void TestEmptyFile()
        {
            ServiceDescription<MockWrapper> desc = new();
            var empty = CursorText.Create(
                new DirectoryInfo(Directory.GetCurrentDirectory()),
                "test",
                $"");
            var result = desc.UpdateFrom(ref empty);
            Assert.AreEqual(ParityParsingState.Void, result);
        }
        [TestMethod]
        public void TestSpaceFile()
        {
            ServiceDescription<MockWrapper> desc = new();
            var empty = CursorText.Create(
                new DirectoryInfo(Directory.GetCurrentDirectory()),
                "test",
                $"""
                   
                """);
            var result = desc.UpdateFrom(ref empty);
            Assert.AreEqual(0, empty.LocalScope.Count);
            Assert.AreEqual(ParityParsingState.Void, result);
        }
        [TestMethod]
        public void TestSimpleDeclaration()
        {
            ServiceDescription<MockWrapper> desc = new();
            var simple = CursorText.Create(
                new DirectoryInfo(Directory.GetCurrentDirectory()),
                "test",
                $"""
                Entry->SomeService();
                """);
            var result = desc.UpdateFrom(ref simple);
            Assert.IsTrue(result > ParityParsingState.Unchanged);
            Assert.IsTrue(simple.LocalScope.TryGetValue("Entry", out var cEntry));
            Assert.IsTrue(cEntry is ServiceDescription<MockWrapper> cDesc && cDesc == desc);
            Assert.IsTrue(simple.LocalScope.TryGetValue("const_Entry", out var consts));
            Assert.IsTrue(consts is ServiceConstantsDescription scDesc && scDesc.Members.Count == 0);
            Assert.AreEqual("Entry", desc.ConstantsDescription.BranchKey);
            Assert.AreEqual("SomeService", desc.ServiceTypeName);
        }


        [TestMethod]
        public void TestConstantsDeclaration()
        {
            ServiceDescription<MockWrapper> desc = new();
            string cdir = Directory.GetCurrentDirectory();
            var simple = CursorText.Create(
                new DirectoryInfo(cdir),
                "test",
                $"""
                Entry->SomeService(yotta = "oy!", terra = 123, stinky = False, path = f"hi.txt");
                """);
            var result = desc.UpdateFrom(ref simple);
            Assert.IsTrue(result > ParityParsingState.Unchanged);
            Assert.IsTrue(simple.LocalScope.TryGetValue("Entry", out var cEntry));
            Assert.IsTrue(cEntry is ServiceDescription<MockWrapper> cDesc && cDesc == desc);
            Assert.IsTrue(simple.LocalScope.TryGetValue("const_Entry", out var consts));
            Assert.IsTrue(consts is ServiceConstantsDescription scDesc && scDesc.Members.Count == 4);
            var assertedConsts = (ServiceConstantsDescription)consts;
            Assert.AreEqual("oy!", assertedConsts.Members.Single(x => x.Key == "yotta").Value.GetValue());
            Assert.AreEqual(123M, assertedConsts.Members.Single(x => x.Key == "terra").Value.GetValue());
            Assert.AreEqual(false, assertedConsts.Members.Single(x => x.Key == "stinky").Value.GetValue());
            (string x, string y) registeredPath = ((string x, string y))assertedConsts.Members.Single(x => x.Key == "path").Value.GetValue();
            Assert.AreEqual(Path.Combine(cdir, "hi.txt"), Path.Combine(registeredPath.x, registeredPath.y));
            Assert.AreEqual("Entry", desc.ConstantsDescription.BranchKey);
            Assert.AreEqual("SomeService", desc.ServiceTypeName);
        }


        [TestMethod]
        public void TestNestingConstantsDeclaration()
        {
            ServiceDescription<MockWrapper> desc = new();
            string cdir = Directory.GetCurrentDirectory();
            var simple = CursorText.Create(
                new DirectoryInfo(cdir),
                "test",
                """
                Entry->SomeService(yotta = "oy!", terra = 123, stinky = False, path = f"hi.txt") {
                    Child->OtherService(exa = "beep");
                    Child2->MoreService(peta = "boop");
                };
                """);
            var result = desc.UpdateFrom(ref simple);
            Assert.IsTrue(result > ParityParsingState.Unchanged);
            Assert.IsTrue(simple.LocalScope.TryGetValue("Entry", out var cEntry));
            Assert.IsTrue(cEntry is ServiceDescription<MockWrapper> cDesc && cDesc == desc);
            Assert.IsTrue(simple.LocalScope.TryGetValue("const_Entry", out var consts));
            Assert.IsTrue(consts is ServiceConstantsDescription scDesc && scDesc.Members.Count == 4);
            var assertedConsts = (ServiceConstantsDescription)consts;
            Assert.AreEqual("oy!", assertedConsts.Members.Single(x => x.Key == "yotta").Value.GetValue());
            Assert.AreEqual(123M, assertedConsts.Members.Single(x => x.Key == "terra").Value.GetValue());
            Assert.AreEqual(false, assertedConsts.Members.Single(x => x.Key == "stinky").Value.GetValue());
            (string x, string y) registeredPath = ((string x, string y))assertedConsts.Members.Single(x => x.Key == "path").Value.GetValue();
            Assert.AreEqual(Path.Combine(cdir, "hi.txt"), Path.Combine(registeredPath.x, registeredPath.y));
            Assert.AreEqual("Entry", desc.ConstantsDescription.BranchKey);
            Assert.AreEqual("SomeService", desc.ServiceTypeName);

            Assert.AreEqual(2, desc.Children.Count);
            Assert.AreEqual("Child", desc.Children[0].ConstantsDescription.BranchKey);
            Assert.AreEqual("Child2", desc.Children[1].ConstantsDescription.BranchKey);
            Assert.AreEqual("OtherService", desc.Children[0].ServiceTypeName);
            Assert.AreEqual("MoreService", desc.Children[1].ServiceTypeName);
        }
        [TestMethod]
        public void TestNestingReferringConstantsDeclaration()
        {
            ServiceDescription<MockWrapper> desc = new();
            string cdir = Directory.GetCurrentDirectory();
            var simple = CursorText.Create(
                new DirectoryInfo(cdir),
                "test",
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
            Assert.IsTrue(cEntry is ServiceDescription<MockWrapper> cDesc && cDesc == desc);
            Assert.IsTrue(simple.LocalScope.TryGetValue("const_Entry", out var consts));
            Assert.IsTrue(consts is ServiceConstantsDescription scDesc && scDesc.Members.Count == 4);
            var assertedConsts = (ServiceConstantsDescription)consts;
            Assert.AreEqual("oy!", assertedConsts.Members.Single(x => x.Key == "yotta").Value.GetValue());
            Assert.AreEqual(123M, assertedConsts.Members.Single(x => x.Key == "terra").Value.GetValue());
            Assert.AreEqual(false, assertedConsts.Members.Single(x => x.Key == "stinky").Value.GetValue());
            (string x, string y) registeredPath = ((string x, string y))assertedConsts.Members.Single(x => x.Key == "path").Value.GetValue();
            Assert.AreEqual(Path.Combine(cdir, "hi.txt"), Path.Combine(registeredPath.x, registeredPath.y));
            Assert.AreEqual("Entry", desc.ConstantsDescription.BranchKey);
            Assert.AreEqual("SomeService", desc.ServiceTypeName);

            Assert.AreEqual(3, desc.Children.Count);
            Assert.AreEqual("Child", desc.Children[0].ConstantsDescription.BranchKey);
            Assert.AreEqual("_EarlyDefine", desc.Children[1].ConstantsDescription.BranchKey);
            Assert.AreEqual("Child2", desc.Children[2].ConstantsDescription.BranchKey);
            Assert.AreEqual("OtherService", desc.Children[0].ServiceTypeName);
            Assert.AreEqual("SecretService", desc.Children[2].Children[0].RedirectsTo?.ServiceTypeName);
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

            ServiceDescription<MockWrapper> desc = new();
            string cdir = Directory.GetCurrentDirectory();
            var simple = CursorText.Create(
                new DirectoryInfo(cdir),
                "test",
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

        [TestMethod]
        public void TestContinueRoundtrip()
        {
            var testText = """
                Entry->SomeService(yotta = "oy!", terra = 123, stinky = False, path = f"hi.txt") {
                    Child->OtherService(exa = "beep");
                    _EarlyDefine->SecretService();
                    Child2->MoreService(peta = "boop"):OtherService() {
                        CallbackBranch->_EarlyDefine;
                    };
                };
                """;

            ServiceDescription<MockWrapper> desc = new();
            string cdir = Directory.GetCurrentDirectory();
            var simple = CursorText.Create(
                new DirectoryInfo(cdir),
                "test",
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
        [TestMethod]
        public void TestConcatRoundtrip()
        {
            var testText = """
                Entry->SomeService(yotta = "oy!", terra = 123, stinky = False, path = f"hi.txt") {
                    Child->OtherService(exa = "beep")
                         & PieService(farts = 44);
                    Baby->OtherService(exa = "beep"):Tubes()
                        & PieService(farts = 44) {
                            Oink->Pig():Hog();
                        };
                    _EarlyDefine->SecretService();
                    Child2->MoreService(peta = "boop"):OtherService() {
                        CallbackBranch->_EarlyDefine;
                    };
                };
                """;

            ServiceDescription<MockWrapper> desc = new();
            string cdir = Directory.GetCurrentDirectory();
            var simple = CursorText.Create(
                new DirectoryInfo(cdir),
                "test",
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
        [TestMethod]
        public void TestMixedConcatRoundtrip()
        {
            var testText = """
                start->Definition():Hold() {
                    Continue->ConsoleOutput():ConstantTextSource(text = "Koetjesrepen")
                            & ConsoleReadLine():Release();
                };
                """;

            ServiceDescription<MockWrapper> desc = new();
            string cdir = Directory.GetCurrentDirectory();
            var simple = CursorText.Create(
                new DirectoryInfo(cdir),
                "test",
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
        [TestMethod]
        public void TestFunkySpacedRoundtrip()
        {
            var testText = """
                start->Definition():Hold() {
                    Continue 
                    -> ConsoleOutput():ConstantTextSource(text = "Koetjesrepen") 
                    &  ConsoleReadLine():Release();
                };
                """;

            var fixedText = """
                start->Definition():Hold() {
                    Continue->ConsoleOutput():ConstantTextSource(text = "Koetjesrepen")
                            & ConsoleReadLine():Release();
                };
                """;

            ServiceDescription<MockWrapper> desc = new();
            string cdir = Directory.GetCurrentDirectory();
            var simple = CursorText.Create(
                new DirectoryInfo(cdir),
                "test",
                testText);
            var result = desc.UpdateFrom(ref simple);

            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            desc.WriteTo(writer);
            writer.Flush();
            ms.Position = 0;
            var reader = new StreamReader(ms);
            var fullText = reader.ReadToEnd();

            Assert.AreEqual(fixedText.Trim(), fullText.Trim());
        }

        [TestMethod]
        public void TestArrayRoundtrip()
        {
            var testText = """
                start->Definition():Hold(items = ["honky", "tonky", "phonky"]) {
                    Continue-> ConsoleOutput():ConstantTextSource(text = "Koetjesrepen")   &  ConsoleReadLine():Release();
                };
                """;

            var fixedText = """
                start->Definition():Hold(items = ["honky", "tonky", "phonky"]) {
                    Continue->ConsoleOutput():ConstantTextSource(text = "Koetjesrepen")
                            & ConsoleReadLine():Release();
                };
                """;

            ServiceDescription<MockWrapper> desc = new();
            string cdir = Directory.GetCurrentDirectory();
            var simple = CursorText.Create(
                new DirectoryInfo(cdir),
                "test",
                testText);
            var result = desc.UpdateFrom(ref simple);

            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            desc.WriteTo(writer);
            writer.Flush();
            ms.Position = 0;
            var reader = new StreamReader(ms);
            var fullText = reader.ReadToEnd();

            Assert.AreEqual(fixedText.Trim(), fullText.Trim());
        }

        [TestMethod]
        public void TestDirtyArrayRoundtrip()
        {
            var testText = """
                start->Definition():Hold(items = [
                "honky",
                , "tonky","phonky",]) {
                    Continue 
                    -> ConsoleOutput():ConstantTextSource(text = "Koetjesrepen") 
                    &  ConsoleReadLine():Release();
                };
                """;

            var fixedText = """
                start->Definition():Hold(items = ["honky", "tonky", "phonky"]) {
                    Continue->ConsoleOutput():ConstantTextSource(text = "Koetjesrepen")
                            & ConsoleReadLine():Release();
                };
                """;

            ServiceDescription<MockWrapper> desc = new();
            string cdir = Directory.GetCurrentDirectory();
            var simple = CursorText.Create(
                new DirectoryInfo(cdir),
                "test",
                testText);
            var result = desc.UpdateFrom(ref simple);

            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            desc.WriteTo(writer);
            writer.Flush();
            ms.Position = 0;
            var reader = new StreamReader(ms);
            var fullText = reader.ReadToEnd();

            Assert.AreEqual(fixedText.Trim(), fullText.Trim());
        }

        [TestMethod]
        public void TestRedirectionsMoreBetter()
        {
            var testText = """
                start->Definition():Hold() {
                    _server->WebServer(prefixes = ["http://localhost:8533/"]):Call(modulefile = "C:\\Users\\deFine\\source\\repos\\Ziewaar.RAD.Doodads.Editor");
                    _instructionLoop->ConsoleReadLine():Option(equals = "stop") {
                        Continue->ConsoleOutput():ConstantTextSource(text = "Received stop instruction")
                                & StopWebServer():_server;
                        NotApplicable->ConsoleOutput():ConstantTextSource(text = "unknown instruction")
                                     & _instructionLoop;
                    };
                    Continue->StartWebServer():_server;
                };
                """;

            ServiceDescription<MockWrapper> desc = new();
            string cdir = Directory.GetCurrentDirectory();
            var simple = CursorText.Create(
                new DirectoryInfo(cdir),
                "test",
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

        [TestMethod]
        public void TestRedirectionsMoreBetterWithAmpersands()
        {
            var testText = """
                start->Definition():Hold() {
                    _instructionLoop->VoidService();
                    _server->WebServer(prefixes = ["http://localhost:8533/"]):Call(modulefile = "C:\\Users\\deFine\\source\\repos\\Ziewaar.RAD.Doodads.Editor")
                           & _instructionLoop;
                    _instructionLoop->ConsoleReadLine():Option(equals = "stop") {
                        Continue->ConsoleOutput():ConstantTextSource(text = "Received stop instruction")
                                & StopWebServer() {
                                    Bla->_server;
                                }
                                & Release();
                        NotApplicable->ConsoleOutput():ConstantTextSource(text = "unknown instruction")
                                     & _instructionLoop;
                    };
                    Continue->StartWebServer():_server;
                };
                """;

            ServiceDescription<MockWrapper> desc = new();
            string cdir = Directory.GetCurrentDirectory();
            var simple = CursorText.Create(
                new DirectoryInfo(cdir),
                "test",
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
