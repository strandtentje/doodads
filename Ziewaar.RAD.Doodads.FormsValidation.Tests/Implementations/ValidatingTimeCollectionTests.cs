namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Implementations;
[TestClass]
public class ValidatingTimeCollectionTests
{
    [TestMethod]
    public void Add_TimeOnly_IntendedBehavior_ShouldPass()
    {
        var c = new ValidatingTimeCollection();
        var t = new TimeOnly(9, 0);

        c.Add(t);

        // Intended behavior: IsSatisfied should be true and item stored.
        Assert.IsTrue(c.IsSatisfied, "Likely missing default initialization to true.");
        Assert.AreEqual(t, c.ValidItems.Cast<TimeOnly>().Single());
    }

    [TestMethod]
    public void Add_InvalidString_ShouldFail()
    {
        var c = new ValidatingTimeCollection();
        c.Add("not-a-time");

        Assert.IsFalse(c.IsSatisfied);
        Assert.AreEqual(0, c.ValidItems.Cast<object>().Count());
    }
}