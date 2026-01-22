#pragma warning disable 67
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
        var ex = Assert.ThrowsExactly<ArgumentException>(sut.Build);
        Assert.AreEqual("StarterFile", ex.ParamName);
        ex = Assert.ThrowsExactly<ArgumentException>(() => sut.SetStarter(testfilename));
        Assert.AreEqual("name", ex.ParamName);
        ex = Assert.ThrowsExactly<ArgumentException>(() => sut.AddFile(testfilename));
        Assert.AreEqual("definitionText", ex.ParamName);
        sut.AddFile(testfilename, "");
        sut.SetStarter(testfilename);
        var output = sut.Build();
        Assert.AreEqual(dir, output.WorkingDirectory);
        Assert.IsEmpty(output.PopulateAssemblies);
        Assert.HasCount(1, output.LoadFiles);
        Assert.AreEqual(testfilename, output.StartFile);
        Assert.IsEmpty(output.RootInteractionMemory);
    }
    [TestMethod]
    public void TestWithAssemblies()
    {
        var dir = Path.GetTempPath();
        var testfilename = Path.GetTempFileName();
        if (File.Exists(Path.Combine(dir, testfilename)))
            File.Delete(Path.Combine(dir, testfilename));
        var sut = BootstrappedStartBuilder.Create(dir);
        var ex = Assert.ThrowsExactly<ArgumentException>(sut.Build);
        Assert.AreEqual("StarterFile", ex.ParamName);
        ex = Assert.ThrowsExactly<ArgumentException>(() => sut.SetStarter(testfilename));
        Assert.AreEqual("name", ex.ParamName);
        ex = Assert.ThrowsExactly<ArgumentException>(() => sut.AddFile(testfilename));
        Assert.AreEqual("definitionText", ex.ParamName);
        sut.AddFile(testfilename, "");
        sut.SetStarter(testfilename);
        sut.AddAssemblyBy<ExceptionCauser>();
        var output = sut.Build();
        Assert.AreEqual(dir, output.WorkingDirectory);
        Assert.HasCount(1, output.PopulateAssemblies);
        Assert.AreEqual(typeof(BootstrapperBuilderTest).Assembly, output.PopulateAssemblies[0]);
        Assert.HasCount(1, output.LoadFiles);
        Assert.AreEqual(testfilename, output.StartFile);
        Assert.IsEmpty(output.RootInteractionMemory);
    }
}