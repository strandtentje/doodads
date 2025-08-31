namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Factories;
[TestClass]
public class OptionsValidatingCollectionFactoryTests
{
    [TestMethod]
    public void Create_WithOptions_AllowsOnlyListedAndNoStreams()
    {
        var factory = new OptionsValidatingCollectionFactory(new[] { "red", "green" });
        var coll = factory.Create();

        coll.Add("red");
        Assert.IsTrue(coll.IsSatisfied);
        CollectionAssert.AreEqual(new object[] { "red" }, coll.ValidItems.Cast<object>().ToArray());

        using var ms = new MemoryStream();
        coll.Add(ms); // should invalidate and clear
        Assert.IsFalse(coll.IsSatisfied);
    }

    [TestMethod]
    public void Create_EmptyOptions_AllowsAnything()
    {
        var factory = new OptionsValidatingCollectionFactory(Array.Empty<string>());
        var coll = factory.Create();

        coll.Add("whatever");
        coll.Add(42);

        Assert.IsTrue(coll.IsSatisfied);
        CollectionAssert.AreEqual(new object[] { "whatever", 42 }, coll.ValidItems.Cast<object>().ToArray());
    }
}