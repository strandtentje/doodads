using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

namespace Ziewaar.RAD.Starter.Test;

[TestClass]
public sealed class QuotingTest
{
    [TestMethod]
    public void EmptyTextInQuotesTest()
    {
        Assert.AreEqual(@"""""", "".PutInQuotes());
    }
    [TestMethod]
    public void SpaceInQuotesTest()
    {
        Assert.AreEqual(@""" """, " ".PutInQuotes());
    }
    [TestMethod]
    public void NameInQuotesTest()
    {
        Assert.AreEqual(@"""JohnCheese""", "JohnCheese".PutInQuotes());
    }
    [TestMethod]
    public void SpaceNameInQuotesTest()
    {
        Assert.AreEqual(@"""John Cheese""", "John Cheese".PutInQuotes());
    }
    [TestMethod]
    public void SlashNameInQuotesTest()
    {
        Assert.AreEqual(@"""John\\Cheese""", "John\\Cheese".PutInQuotes());
    }
    [TestMethod]
    public void QuoteNameInQuotesTest()
    {
        Assert.AreEqual(@"""John\""Cheese""", @"John""Cheese".PutInQuotes());
    }
}

public class ExceptionCauser : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        throw new NotImplementedException();
    }

    public void HandleFatal(IInteraction source, Exception ex)
    {
        throw new NotImplementedException();
    }
}

[TestClass]
public class BootstrapperBuilderTest
{
    [TestMethod]
    public void TestNoBootstrap()
    {
        var dir = Path.GetTempPath();
        var testfilename = Path.GetTempFileName();
        if (File.Exists(Path.Combine(dir, testfilename)))
            File.Delete(Path.Combine(dir, testfilename));
        var sut = BootstrappedStartBuilder.Create(dir);
        var ex = Assert.ThrowsException<ArgumentException>(sut.Build);
        Assert.AreEqual("StarterFile", ex.ParamName);
        ex = Assert.ThrowsException<ArgumentException>(() => sut.SetStarter(testfilename));
        Assert.AreEqual("name", ex.ParamName);
        ex = Assert.ThrowsException<ArgumentException>(() => sut.AddFile(testfilename));
        Assert.AreEqual("definitionText", ex.ParamName);
        sut.AddFile(testfilename, "");
        sut.SetStarter(testfilename);
        var output = sut.Build();
        Assert.AreEqual(dir, output.WorkingDirectory);
        Assert.AreEqual(0, output.PopulateAssemblies.Length);
        Assert.AreEqual(1, output.LoadFiles.Count);
        Assert.AreEqual(testfilename, output.StartFile);
        Assert.AreEqual(0, output.RootInteractionMemory.Count);
    }
    [TestMethod]
    public void TestWithAssemblies()
    {
        var dir = Path.GetTempPath();
        var testfilename = Path.GetTempFileName();
        if (File.Exists(Path.Combine(dir, testfilename)))
            File.Delete(Path.Combine(dir, testfilename));
        var sut = BootstrappedStartBuilder.Create(dir);
        var ex = Assert.ThrowsException<ArgumentException>(sut.Build);
        Assert.AreEqual("StarterFile", ex.ParamName);
        ex = Assert.ThrowsException<ArgumentException>(() => sut.SetStarter(testfilename));
        Assert.AreEqual("name", ex.ParamName);
        ex = Assert.ThrowsException<ArgumentException>(() => sut.AddFile(testfilename));
        Assert.AreEqual("definitionText", ex.ParamName);
        sut.AddFile(testfilename, "");
        sut.SetStarter(testfilename);
        sut.AddAssemblyBy<ExceptionCauser>();
        var output = sut.Build();
        Assert.AreEqual(dir, output.WorkingDirectory);
        Assert.AreEqual(1, output.PopulateAssemblies.Length);
        Assert.AreEqual(typeof(BootstrapperBuilderTest).Assembly, output.PopulateAssemblies[0]);
        Assert.AreEqual(1, output.LoadFiles.Count);
        Assert.AreEqual(testfilename, output.StartFile);
        Assert.AreEqual(0, output.RootInteractionMemory.Count);
    }
    [TestMethod]
    public void TestWithAssembliesAndRuntime()
    {
        var dir = Path.GetTempPath();
        var testfilename = Path.GetTempFileName();
        if (File.Exists(Path.Combine(dir, testfilename)))
            File.Delete(Path.Combine(dir, testfilename));
        var sut = BootstrappedStartBuilder.Create(dir);
        var ex = Assert.ThrowsException<ArgumentException>(sut.Build);
        Assert.AreEqual("StarterFile", ex.ParamName);
        ex = Assert.ThrowsException<ArgumentException>(() => sut.SetStarter(testfilename));
        Assert.AreEqual("name", ex.ParamName);
        ex = Assert.ThrowsException<ArgumentException>(() => sut.AddFile(testfilename));
        Assert.AreEqual("definitionText", ex.ParamName);
        sut.AddFile(testfilename, "");
        sut.SetStarter(testfilename);
        sut.SetRuntimeBy<Ziewaar.RAD.Doodads.RuntimeForDotnetCore.Program>();
        sut.AddAssemblyBy<ExceptionCauser>();
        var output = sut.BuildProcessStart();
        Assert.AreEqual(dir, output.WorkingDirectory);
        Assert.IsTrue(output.FileName.Contains("RuntimeForDotnetCore.exe"));
        Assert.IsTrue(output.Arguments.Contains("Starter.Test"));
        Assert.IsTrue(output.Arguments.Contains((new FileInfo(testfilename)).Name));
    }
}