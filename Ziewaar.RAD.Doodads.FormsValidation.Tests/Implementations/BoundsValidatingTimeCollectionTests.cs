using Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Implementations.Bounding;

namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Implementations;
[TestClass]
public class BoundsValidatingTimeCollectionTests
{
    private readonly TimeOnly Lower = new TimeOnly(8, 0, 0);
    private readonly TimeOnly Upper = new TimeOnly(17, 30, 0);

    [TestMethod]
    public void Add_TimeOnlyWithinBounds_ShouldBeValid()
    {
        var c = new BoundsValidatingTimeCollection(Lower, Upper);

        var t = new TimeOnly(9, 15);
        c.Add(t);

        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual(t, c.ValidItems.Cast<TimeOnly>().Single());
    }

    [TestMethod]
    public void Add_StringParsableWithinBounds_ShouldBeValid()
    {
        var c = new BoundsValidatingTimeCollection(Lower, Upper);

        c.Add("16:45");

        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual(new TimeOnly(16, 45), c.ValidItems.Cast<TimeOnly>().Single());
    }

    [TestMethod]
    public void Add_ExactlyAtBounds_ShouldBeValid()
    {
        var c = new BoundsValidatingTimeCollection(Lower, Upper);

        c.Add(Lower);
        c.Add(Upper);

        Assert.IsTrue(c.IsSatisfied);
        var items = c.ValidItems.Cast<TimeOnly>().ToList();
        CollectionAssert.AreEqual(new[] { Lower, Upper }, items);
    }

    [TestMethod]
    public void Add_OutOfRange_ShouldInvalidate_AndStoreNothing()
    {
        var c = new BoundsValidatingTimeCollection(Lower, Upper);

        c.Add(new TimeOnly(7, 59));

        Assert.IsFalse(c.IsSatisfied);
        Assert.AreEqual(0, c.ValidItems.Cast<object>().Count());
    }

    [TestMethod]
    public void Add_InvalidString_ShouldInvalidate_AndStoreNothing()
    {
        var c = new BoundsValidatingTimeCollection(Lower, Upper);

        c.Add("not-a-time");

        Assert.IsFalse(c.IsSatisfied);
        Assert.AreEqual(0, c.ValidItems.Cast<object>().Count());
    }

    [TestMethod]
    public void Add_ValidThenInvalid_ShouldStopAcceptingFurtherItems()
    {
        var c = new BoundsValidatingTimeCollection(Lower, Upper);

        c.Add(new TimeOnly(10, 0));
        c.Add("oops");
        c.Add(new TimeOnly(12, 0));

        Assert.IsFalse(c.IsSatisfied);
        var items = c.ValidItems.Cast<TimeOnly>().ToList();
        Assert.HasCount(1, items);
        Assert.AreEqual(new TimeOnly(10, 0), items[0]);
    }
}