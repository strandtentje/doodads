using System.Xml.Resolvers;

namespace Ziewaar.RAD.Doodads.RKOP.Testing
{
    [TestClass]
    public sealed class RkopParserRoundtrip
    {
        [TestMethod]
        public void TestEmptyFile()
        {
            ServiceDescription<MockWrapper> desc = new();
            var empty = CursorText.Create(
                new DirectoryInfo(Directory.GetCurrentDirectory()),
                "test",
                $"");
            var result = desc.UpdateFrom("test", ref empty);
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
            var result = desc.UpdateFrom("test", ref empty);
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
                SomeService();
                """);
            var result = desc.UpdateFrom("test", ref simple);
            Assert.IsTrue(result > ParityParsingState.Unchanged);
            Assert.IsTrue(simple.LocalScope.TryGetValue("test", out var cEntry));
            Assert.IsTrue(cEntry is ServiceDescription<MockWrapper> cDesc && cDesc == desc);

            Assert.AreEqual("SomeService", desc.Constructor.ServiceTypeName);
            Assert.AreEqual(0, desc.Constructor.Constants.Members.Count);
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
                SomeService(yotta = "oy!", terra = 123, stinky = False, path = f"hi.txt");
                """);
            var result = desc.UpdateFrom("test", ref simple);
            Assert.IsTrue(result > ParityParsingState.Unchanged);
            var consts = desc.Constructor.Constants;
            Assert.IsTrue(consts.Members.Count == 4);
            Assert.AreEqual("oy!", consts.Members.Single(x => x.Key == "yotta").Value.GetValue());
            Assert.AreEqual(123M, consts.Members.Single(x => x.Key == "terra").Value.GetValue());
            Assert.AreEqual(false, consts.Members.Single(x => x.Key == "stinky").Value.GetValue());
            (string x, string y) registeredPath = (FileInWorkingDirectory)consts.Members.Single(x => x.Key == "path").Value.GetValue();
            string? registeredPathCombined = consts.Members.Single(x => x.Key == "path").Value?.GetValue().ToString();
            Assert.IsNotNull(registeredPathCombined);
            Assert.IsTrue(registeredPathCombined.EndsWith("hi.txt"));
            Assert.IsTrue(registeredPathCombined.StartsWith(cdir));
            Assert.AreEqual(Path.Combine(cdir, "hi.txt"), Path.Combine(registeredPath.x, registeredPath.y));
            Assert.AreEqual("SomeService", desc.Constructor.ServiceTypeName);
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
                SomeService(yotta = "oy!", terra = 123, stinky = False, path = f"hi.txt") {
                    Child->OtherService(exa = "beep");
                    Child2->MoreService(peta = "boop");
                };
                """);
            var result = desc.UpdateFrom("test", ref simple);
            Assert.IsTrue(result > ParityParsingState.Unchanged);
            Assert.IsTrue(simple.LocalScope.TryGetValue("test", out var cEntry));
            Assert.IsTrue(cEntry is ServiceDescription<MockWrapper> cDesc && cDesc == desc);

            var consts = desc.Constructor.Constants;

            Assert.AreEqual("oy!", consts.Members.Single(x => x.Key == "yotta").Value.GetValue());
            Assert.AreEqual(123M, consts.Members.Single(x => x.Key == "terra").Value.GetValue());
            Assert.AreEqual(false, consts.Members.Single(x => x.Key == "stinky").Value.GetValue());
            (string x, string y) registeredPath = (FileInWorkingDirectory)consts.Members.Single(x => x.Key == "path").Value.GetValue();
            Assert.AreEqual(Path.Combine(cdir, "hi.txt"), Path.Combine(registeredPath.x, registeredPath.y));
            Assert.AreEqual("SomeService", desc.Constructor.ServiceTypeName);

            var child = desc.
                Query<ServiceExpression<MockWrapper>>(x => x.CurrentNameInScope == "Child").
                QueryAllServices<ServiceExpression<MockWrapper>, ServiceDescription<MockWrapper>, MockWrapper>(x => true).
                SingleOrDefault();

            var child2 = desc.
                Query<ServiceExpression<MockWrapper>>(x => x.CurrentNameInScope == "Child2").
                QueryAllServices<ServiceExpression<MockWrapper>, ServiceDescription<MockWrapper>, MockWrapper>(x => true).
                SingleOrDefault();

            Assert.IsNotNull(child);
            Assert.IsNotNull(child2);
            Assert.IsNotNull(desc.Children.Branches);
            Assert.AreEqual(2, desc.Children.Branches.Count);
            Assert.AreEqual("OtherService", child.Constructor.ServiceTypeName);
            Assert.AreEqual("MoreService", child2.Constructor.ServiceTypeName);
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
                SomeService(yotta = "oy!", terra = 123, stinky = False, path = f"hi.txt") {
                    Child->OtherService(exa = "beep");
                    _EarlyDefine->SecretService();
                    Child2->MoreService(peta = "boop") {
                        CallbackBranch->_EarlyDefine;
                    };
                };
                """);
            var result = desc.UpdateFrom("test", ref simple);
            Assert.IsTrue(result > ParityParsingState.Unchanged);
            Assert.IsTrue(simple.LocalScope.TryGetValue("test", out var cEntry));
            Assert.IsTrue(cEntry is ServiceDescription<MockWrapper> cDesc && cDesc == desc);

            var consts = desc.Constructor.Constants;
            Assert.AreEqual("oy!", consts.Members.Single(x => x.Key == "yotta").Value.GetValue());
            Assert.AreEqual(123M, consts.Members.Single(x => x.Key == "terra").Value.GetValue());
            Assert.AreEqual(false, consts.Members.Single(x => x.Key == "stinky").Value.GetValue());
            (string x, string y) registeredPath = (FileInWorkingDirectory)consts.Members.Single(x => x.Key == "path").Value.GetValue();
            Assert.AreEqual(Path.Combine(cdir, "hi.txt"), Path.Combine(registeredPath.x, registeredPath.y));

            Assert.AreEqual("SomeService", desc.Constructor.ServiceTypeName);

            Assert.IsNotNull(desc.Children.Branches);
            Assert.AreEqual(3, desc.Children.Branches.Count);

            var childSet = desc.
                Query<ServiceExpression<MockWrapper>>(x => x.CurrentNameInScope == "Child").
                QueryAllServices<ServiceExpression<MockWrapper>, ServiceDescription<MockWrapper>, MockWrapper>();
            var child = childSet.SingleOrDefault();
            var earlyDefineSet = desc.Query<ServiceExpression<MockWrapper>>(x => x.CurrentNameInScope == "_EarlyDefine").
                QueryAllServices<ServiceExpression<MockWrapper>, SerializableRedirection<MockWrapper>, MockWrapper>().
                QueryAllServices<ServiceExpression<MockWrapper>, ServiceDescription<MockWrapper>, MockWrapper>();
            var earlyDefine = earlyDefineSet.SingleOrDefault();
            var child2set = desc.Query<ServiceExpression<MockWrapper>>(x => x.CurrentNameInScope == "Child2").
                QueryAllServices<ServiceExpression<MockWrapper>, ServiceDescription<MockWrapper>, MockWrapper>();
            var child2 = child2set.SingleOrDefault();
            var callBackSet = desc.Query<ServiceExpression<MockWrapper>>(x => x.CurrentNameInScope == "CallbackBranch").
                QueryAllServices<ServiceExpression<MockWrapper>, ServiceDescription<MockWrapper>, MockWrapper>();
            var callBack = callBackSet.SingleOrDefault();
            var callbackDefinitionSet = callBack?.Query<ServiceDescription<MockWrapper>>();
            var callbackDefinition = callbackDefinitionSet?.SingleOrDefault();

            Assert.IsNotNull(child);
            Assert.IsNotNull(earlyDefine);
            Assert.IsNotNull(child2);
            Assert.IsNotNull(callBack);
            Assert.IsNotNull(callbackDefinition);

            Assert.AreEqual("OtherService", child.Constructor.ServiceTypeName);
            Assert.AreEqual("SecretService", earlyDefine.Constructor.ServiceTypeName);
            Assert.AreEqual("MoreService", child2.Constructor.ServiceTypeName);
            Assert.AreEqual(earlyDefine, callbackDefinition);
        }
        [TestMethod]
        public void TestRoundtrip()
        {
            var testText = """
                SomeService(yotta = "oy!", terra = 123, stinky = False, path = f"hi.txt") {
                    Child->OtherService(exa = "beep");
                    _EarlyDefine->SecretService();
                    Child2->MoreService(peta = "boop") {
                        CallbackBranch->_EarlyDefine;
                    };
                }
                """;

            UnconditionalSerializableServiceSeries<MockWrapper> desc = new();
            string cdir = Directory.GetCurrentDirectory();
            var simple = CursorText.Create(
                new DirectoryInfo(cdir),
                "test",
                testText);
            var result = desc.UpdateFrom("test", ref simple);

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
                SomeService(yotta = "oy!", terra = 123, stinky = False, path = f"hi.txt") {
                    Child->OtherService(exa = "beep");
                    _EarlyDefine->SecretService();
                    Child2->MoreService(peta = "boop"):OtherService() {
                        CallbackBranch->_EarlyDefine;
                    };
                }
                """;

            UnconditionalSerializableServiceSeries<MockWrapper> desc = new();
            string cdir = Directory.GetCurrentDirectory();
            var simple = CursorText.Create(
                new DirectoryInfo(cdir),
                "test",
                testText);
            var result = desc.UpdateFrom("test", ref simple);

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
                SomeService(yotta = "oy!", terra = 123, stinky = False, path = f"hi.txt") {
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
                }
                """;

            UnconditionalSerializableServiceSeries<MockWrapper> desc = new();
            string cdir = Directory.GetCurrentDirectory();
            var simple = CursorText.Create(
                new DirectoryInfo(cdir),
                "test",
                testText);
            var result = desc.UpdateFrom("test", ref simple);

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
                Definition():Hold() {
                    Continue->ConsoleOutput():ConstantTextSource(text = "Koetjesrepen")
                            & ConsoleReadLine():Release();
                }
                """;

            UnconditionalSerializableServiceSeries<MockWrapper> desc = new();
            string cdir = Directory.GetCurrentDirectory();
            var simple = CursorText.Create(
                new DirectoryInfo(cdir),
                "test",
                testText);
            var result = desc.UpdateFrom("test", ref simple);

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
                Definition():Hold() {
                    Continue 
                    -> ConsoleOutput():ConstantTextSource(text = "Koetjesrepen") 
                    &  ConsoleReadLine():Release();
                };
                """;

            var fixedText = """
                Definition():Hold() {
                    Continue->ConsoleOutput():ConstantTextSource(text = "Koetjesrepen")
                            & ConsoleReadLine():Release();
                }
                """;

            UnconditionalSerializableServiceSeries<MockWrapper> desc = new();
            string cdir = Directory.GetCurrentDirectory();
            var simple = CursorText.Create(
                new DirectoryInfo(cdir),
                "test",
                testText);
            var result = desc.UpdateFrom("test", ref simple);

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
                Definition():Hold(items = ["honky", "tonky", "phonky"]) {
                    Continue-> ConsoleOutput():ConstantTextSource(text = "Koetjesrepen")   &  ConsoleReadLine():Release();
                };
                """;

            var fixedText = """
                Definition():Hold(items = ["honky", "tonky", "phonky"]) {
                    Continue->ConsoleOutput():ConstantTextSource(text = "Koetjesrepen")
                            & ConsoleReadLine():Release();
                }
                """;

            UnconditionalSerializableServiceSeries<MockWrapper> desc = new();
            string cdir = Directory.GetCurrentDirectory();
            var simple = CursorText.Create(
                new DirectoryInfo(cdir),
                "test",
                testText);
            var result = desc.UpdateFrom("test", ref simple);

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
                Definition():Hold(items = [
                "honky",
                , "tonky","phonky",]) {
                    Continue 
                    -> ConsoleOutput():ConstantTextSource(text = "Koetjesrepen") 
                    &  ConsoleReadLine():Release();
                }
                """;

            var fixedText = """
                Definition():Hold(items = ["honky", "tonky", "phonky"]) {
                    Continue->ConsoleOutput():ConstantTextSource(text = "Koetjesrepen")
                            & ConsoleReadLine():Release();
                }
                """;

            UnconditionalSerializableServiceSeries<MockWrapper> desc = new();
            string cdir = Directory.GetCurrentDirectory();
            var simple = CursorText.Create(
                new DirectoryInfo(cdir),
                "test",
                testText);
            var result = desc.UpdateFrom("test", ref simple);

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
                Definition():Hold() {
                    _server->WebServer(prefixes = ["http://localhost:8533/"]):Call(modulefile = "C:\\Users\\deFine\\source\\repos\\Ziewaar.RAD.Doodads.Editor");
                    _instructionLoop->ConsoleReadLine():Option(equals = "stop") {
                        Continue->ConsoleOutput():ConstantTextSource(text = "Received stop instruction")
                                & StopWebServer():_server;
                        NotApplicable->ConsoleOutput():ConstantTextSource(text = "unknown instruction");
                    };
                    Continue->StartWebServer():_server;
                }
                """;

            UnconditionalSerializableServiceSeries<MockWrapper> desc = new();
            string cdir = Directory.GetCurrentDirectory();
            var simple = CursorText.Create(
                new DirectoryInfo(cdir),
                "test",
                testText);
            var result = desc.UpdateFrom("test", ref simple);

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
                Definition():Hold() {
                    _instructionLoop->ConsoleReadLine():Option(equals = "stop") {
                        Continue->ConsoleOutput():ConstantTextSource(text = "Received stop instruction")
                                & StopWebServer() {
                                    Bla->VoidService();
                                }
                                & Release();
                        NotApplicable->ConsoleOutput():ConstantTextSource(text = "unknown instruction");
                    };
                    _server->WebServer(prefixes = ["http://localhost:8533/"]):Call(modulefile = "C:\\Users\\deFine\\source\\repos\\Ziewaar.RAD.Doodads.Editor")
                           & _instructionLoop;
                    Continue->StartWebServer():_server;
                }
                """;

            UnconditionalSerializableServiceSeries<MockWrapper> desc = new();
            string cdir = Directory.GetCurrentDirectory();
            var simple = CursorText.Create(
                new DirectoryInfo(cdir),
                "test",
                testText);
            var result = desc.UpdateFrom("test", ref simple);

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
