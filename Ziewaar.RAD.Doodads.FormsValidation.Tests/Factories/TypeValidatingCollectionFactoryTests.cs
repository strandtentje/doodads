namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Factories;
[TestClass]
public class TypeValidatingCollectionFactoryTests
{
    [TestMethod]
    public void TypeFactory_InputColor_ReturnsColorValidator()
    {
        var typeFactory = new TypeValidatingCollectionFactory(fieldTag: "input", fieldType: "color");
        var coll = typeFactory.Create();
        coll.Add("#00FFA0");
        Assert.IsTrue(coll.IsSatisfied);
        Assert.AreEqual("#00FFA0", coll.ValidItems.Cast<string>().Single());
    }

    [TestMethod]
    public void TypeFactory_InputNumber_ReturnsNumberValidator()
    {
        var typeFactory = new TypeValidatingCollectionFactory(fieldTag: "input", fieldType: "number");
        var coll = typeFactory.Create();
        coll.Add("42");
        Assert.IsTrue(coll.IsSatisfied);
        Assert.AreEqual(42m, coll.ValidItems.Cast<decimal>().Single());
    }

    [TestMethod]
    public void TypeFactory_FileType_ReturnsFileValidator()
    {
        var typeFactory = new TypeValidatingCollectionFactory(fieldTag: "input", fieldType: "file");
        var coll = typeFactory.Create();
        using var ms = new MemoryStream();
        coll.Add(ms);
        Assert.IsTrue(coll.IsSatisfied);
        Assert.AreSame(ms, coll.ValidItems.Cast<object>().Single());
    }

    [TestMethod]
    public void TypeFactory_Unknown_ReturnsNonValidating()
    {
        var typeFactory = new TypeValidatingCollectionFactory(fieldTag: "input", fieldType: "unknown");
        var coll = typeFactory.Create();
        coll.Add("anything");
        Assert.IsTrue(coll.IsSatisfied);
        Assert.AreEqual("anything", coll.ValidItems.Cast<object>().Single());
    }
}