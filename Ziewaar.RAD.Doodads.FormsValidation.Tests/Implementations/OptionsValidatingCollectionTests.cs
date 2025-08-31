namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Implementations;
[TestClass]
public class OptionsValidatingCollectionTests
{
    [TestMethod]
    public void Add_EmptyOptions_AllowsAnything()
    {
        var c = new OptionsValidatingCollection(Array.Empty<string>());

        c.Add("whatever");
        c.Add(42);

        Assert.IsTrue(c.IsSatisfied);
        var items = c.ValidItems.Cast<object>().ToList();
        CollectionAssert.AreEqual(new object[] { "whatever", 42 }, items);
    }

    [TestMethod]
    public void Add_WithOptions_ValidValue_ShouldPass()
    {
        var c = new OptionsValidatingCollection(new[] { "red", "green", "blue" });

        c.Add("green");

        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual("green", c.ValidItems.Cast<object>().Single());
    }

    [TestMethod]
    public void Add_WithOptions_StreamIsNotAllowed_ShouldFailAndClear()
    {
        var c = new OptionsValidatingCollection(new[] { "x", "y" });

        using var ms = new MemoryStream();
        c.Add(ms);

        Assert.IsFalse(c.IsSatisfied);
        Assert.AreEqual(0, c.ValidItems.Cast<object>().Count());
    }

    [TestMethod]
    public void Add_WithOptions_InvalidValue_ShouldFailAndClear()
    {
        var c = new OptionsValidatingCollection(new[] { "x", "y" });

        c.Add("x"); // ok
        c.Add("z"); // not allowed -> clear

        Assert.IsFalse(c.IsSatisfied);
    }
}