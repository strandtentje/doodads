namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Implementations;
[TestClass]
public class BoundsValidatingNumberCollectionTests
{
    private readonly decimal Lower = -10m;
    private readonly decimal Upper = 10m;

    [TestMethod]
    public void Add_DecimalWithinBounds_ShouldBeValid()
    {
        var c = new BoundsValidatingNumberCollection(Lower, Upper);

        c.Add(3.5m);

        Assert.IsTrue(c.IsSatisfied);
        var v = c.ValidItems.Cast<decimal>().Single();
        Assert.AreEqual(3.5m, v);
    }

    [TestMethod]
    public void Add_StringParsableWithinBounds_ShouldBeValid()
    {
        var c = new BoundsValidatingNumberCollection(Lower, Upper);

        c.Add("9.75");

        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual(9.75m, c.ValidItems.Cast<decimal>().Single());
    }

    [TestMethod]
    public void Add_ExactlyAtBounds_ShouldBeValid()
    {
        var c = new BoundsValidatingNumberCollection(Lower, Upper);

        c.Add(Lower);
        c.Add(Upper);

        Assert.IsTrue(c.IsSatisfied);
        var items = c.ValidItems.Cast<decimal>().ToList();
        CollectionAssert.AreEqual(new[] { Lower, Upper }, items);
    }

    [TestMethod]
    public void Add_OutOfRange_ShouldInvalidate_AndStoreNothing()
    {
        var c = new BoundsValidatingNumberCollection(Lower, Upper);

        c.Add(11m);

        Assert.IsFalse(c.IsSatisfied);
        Assert.AreEqual(0, c.ValidItems.Cast<object>().Count());
    }

    [TestMethod]
    public void Add_InvalidString_ShouldInvalidate_AndStoreNothing()
    {
        var c = new BoundsValidatingNumberCollection(Lower, Upper);

        c.Add("not-a-number");

        Assert.IsFalse(c.IsSatisfied);
        Assert.AreEqual(0, c.ValidItems.Cast<object>().Count());
    }

    [TestMethod]
    public void Add_ValidThenInvalid_ShouldStopAcceptingFurtherItems()
    {
        var c = new BoundsValidatingNumberCollection(Lower, Upper);

        c.Add(0m);
        c.Add("bad");
        c.Add(5m);

        Assert.IsFalse(c.IsSatisfied);
        var items = c.ValidItems.Cast<decimal>().ToList();
        Assert.AreEqual(1, items.Count);
        Assert.AreEqual(0m, items[0]);
    }
}
