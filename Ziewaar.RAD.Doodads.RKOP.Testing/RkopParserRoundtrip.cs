using System.Xml.Resolvers;
using Ziewaar.RAD.Doodads.RKOP.Blocks;
using Ziewaar.RAD.Doodads.RKOP.Constructor;
using Ziewaar.RAD.Doodads.RKOP.SeriesParsers;
using Ziewaar.RAD.Doodads.RKOP.Text;

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
            Assert.IsFalse(result);
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
            Assert.IsFalse(result);
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
            Assert.IsTrue(result);
            Assert.IsTrue(simple.LocalScope.TryGetValue("test", out var cEntry));
            Assert.IsTrue(cEntry is ServiceDescription<MockWrapper> cDesc && cDesc == desc);

            Assert.AreEqual("SomeService", desc.CurrentConstructor.ServiceTypeName);
            Assert.AreEqual(0, desc.CurrentConstructor.ConstantsList.Count);
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
                SomeService("primary", yotta = "oy!", terra = 123, stinky = False, path = f"hi.txt");
                """);
            var result = desc.UpdateFrom("test", ref simple);
            Assert.IsTrue(result);
            var consts = desc.CurrentConstructor.ConstantsList;
            Assert.IsTrue(consts.Count == 4);
            Assert.AreEqual("primary", desc.CurrentConstructor.PrimarySettingValue);
            Assert.AreEqual("oy!", consts["yotta"]);
            Assert.AreEqual(123M, consts["terra"]);
            Assert.AreEqual(false, consts["stinky"]);
            (string x, string y) registeredPath = (FileInWorkingDirectory)consts["path"];
            string? registeredPathCombined = consts["path"].ToString();
            Assert.IsNotNull(registeredPathCombined);
            Assert.IsTrue(registeredPathCombined.EndsWith("hi.txt"));
            Assert.IsTrue(registeredPathCombined.StartsWith(cdir));
            Assert.AreEqual(Path.Combine(cdir, "hi.txt"), Path.Combine(registeredPath.x, registeredPath.y));
            Assert.AreEqual("SomeService", desc.CurrentConstructor.ServiceTypeName);
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
            Assert.IsTrue(result);
            Assert.IsTrue(simple.LocalScope.TryGetValue("test", out var cEntry));
            Assert.IsTrue(cEntry is ServiceDescription<MockWrapper> cDesc && cDesc == desc);

            var consts = desc.CurrentConstructor.ConstantsList;

            Assert.AreEqual("oy!", consts["yotta"]);
            Assert.AreEqual(123M, consts["terra"]);
            Assert.AreEqual(false, consts["stinky"]);
            (string x, string y) registeredPath = (FileInWorkingDirectory)consts["path"];
            Assert.AreEqual(Path.Combine(cdir, "hi.txt"), Path.Combine(registeredPath.x, registeredPath.y));
            Assert.AreEqual("SomeService", desc.CurrentConstructor.ServiceTypeName);

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
            Assert.AreEqual("OtherService", child.CurrentConstructor.ServiceTypeName);
            Assert.AreEqual("MoreService", child2.CurrentConstructor.ServiceTypeName);
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
                    Child2->MoreService(peta = "boop") {
                        CallbackBranch->SecretService();
                    };
                };
                """);
            var result = desc.UpdateFrom("test", ref simple);
            Assert.IsTrue(result);
            Assert.IsTrue(simple.LocalScope.TryGetValue("test", out var cEntry));
            Assert.IsTrue(cEntry is ServiceDescription<MockWrapper> cDesc && cDesc == desc);

            var consts = desc.CurrentConstructor.ConstantsList;
            Assert.AreEqual("oy!", consts["yotta"]);
            Assert.AreEqual(123M, consts["terra"]);
            Assert.AreEqual(false, consts["stinky"]);
            (string x, string y) registeredPath = (FileInWorkingDirectory)consts["path"];
            Assert.AreEqual(Path.Combine(cdir, "hi.txt"), Path.Combine(registeredPath.x, registeredPath.y));

            Assert.AreEqual("SomeService", desc.CurrentConstructor.ServiceTypeName);

            Assert.IsNotNull(desc.Children.Branches);
            Assert.AreEqual(2, desc.Children.Branches.Count);

            var childSet = desc.
                Query<ServiceExpression<MockWrapper>>(x => x.CurrentNameInScope == "Child").
                QueryAllServices<ServiceExpression<MockWrapper>, ServiceDescription<MockWrapper>, MockWrapper>();
            var child = childSet.SingleOrDefault();
            /*var earlyDefineSet = desc.Query<ServiceExpression<MockWrapper>>(x => x.CurrentNameInScope == "_EarlyDefine").
                QueryAllServices<ServiceExpression<MockWrapper>, SerializableRedirection<MockWrapper>, MockWrapper>().
                QueryAllServices<ServiceExpression<MockWrapper>, ServiceDescription<MockWrapper>, MockWrapper>();*
            var earlyDefine = earlyDefineSet.SingleOrDefault();*/
            var child2set = desc.Query<ServiceExpression<MockWrapper>>(x => x.CurrentNameInScope == "Child2").
                QueryAllServices<ServiceExpression<MockWrapper>, ServiceDescription<MockWrapper>, MockWrapper>();
            var child2 = child2set.SingleOrDefault();
            var callBackSet = desc.Query<ServiceExpression<MockWrapper>>(x => x.CurrentNameInScope == "CallbackBranch").
                QueryAllServices<ServiceExpression<MockWrapper>, ServiceDescription<MockWrapper>, MockWrapper>();
            var callBack = callBackSet.SingleOrDefault();
            var callbackDefinitionSet = callBack?.Query<ServiceDescription<MockWrapper>>();
            var callbackDefinition = callbackDefinitionSet?.SingleOrDefault();

            Assert.IsNotNull(child);
            //Assert.IsNotNull(earlyDefine);
            Assert.IsNotNull(child2);
            Assert.IsNotNull(callBack);
            Assert.IsNotNull(callbackDefinition);

            Assert.AreEqual("OtherService", child.CurrentConstructor.ServiceTypeName);
            //Assert.AreEqual("SecretService", earlyDefine.Constructor.ServiceTypeName);
            Assert.AreEqual("MoreService", child2.CurrentConstructor.ServiceTypeName);
            //Assert.AreEqual(earlyDefine, callbackDefinition);
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
                    Child2->MoreService("primary", peta = "boop"):OtherService() {
                        CallbackBranch->SecretService();
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
                    Child2->MoreService(peta = "boop"):OtherService() {
                        CallbackBranch->SecretService();
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
    }
}
