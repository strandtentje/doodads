namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Implementations;
[TestClass]
public class CountingValidatingCollectionTests
{
    [TestMethod]
    public void IsSatisfied_WithinBounds_ShouldBeTrue()
    {
        var c = new CountingValidatingCollection(lowerValueCountLimit: 1, upperValueCountLimit: 3);
        c.Add("a");
        c.Add("b");

        Assert.IsTrue(c.IsSatisfied);
    }

    [TestMethod]
    public void IsSatisfied_BelowLower_ShouldBeFalse()
    {
        var c = new CountingValidatingCollection(2, 5);
        c.Add("only-one");

        Assert.IsFalse(c.IsSatisfied);
    }

    [TestMethod]
    public void IsSatisfied_AboveUpper_ShouldBeFalse()
    {
        var c = new CountingValidatingCollection(1, 2);
        c.Add("a");
        c.Add("b");
        c.Add("c");

        Assert.IsFalse(c.IsSatisfied);
    }
}