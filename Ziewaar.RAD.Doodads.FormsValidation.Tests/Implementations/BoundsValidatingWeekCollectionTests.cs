namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Implementations;
[TestClass]
public class BoundsValidatingWeekCollectionTests
{
    // Bounds cover all ISO weeks of 2020
    private readonly DateOnly Lower = new DateOnly(2019, 12, 30); // Monday of ISO week 1, 2020
    private readonly DateOnly Upper = new DateOnly(2021, 1, 3);   // Sunday of ISO week 53, 2020

    [TestMethod]
    public void Add_DateOnlyWithinBounds_ShouldBeAcceptedAsWeekStart()
    {
        var c = new BoundsValidatingWeekCollection(Lower, Upper);

        // provide a DateOnly that is itself within bounds
        var d = new DateOnly(2020, 6, 15); // Monday of ISO week 25
        c.Add(d);

        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual(d, c.ValidItems.Cast<DateOnly>().Single());
    }

    [TestMethod]
    public void Add_ParsableWeekStringWithinBounds_ShouldStoreWeekStart()
    {
        var c = new BoundsValidatingWeekCollection(Lower, Upper);

        c.Add("2020-W25");

        Assert.IsTrue(c.IsSatisfied);
        var stored = c.ValidItems.Cast<DateOnly>().Single();
        Assert.AreEqual(new DateOnly(2020, 6, 15), stored); // Monday of ISO week 25, 2020
    }

    [TestMethod]
    public void Add_ExactlyAtLowerAndUpperBounds_ShouldBeValid()
    {
        var c = new BoundsValidatingWeekCollection(Lower, Upper);

        c.Add("2020-W01"); // Monday 2019-12-30
        c.Add("2020-W53"); // Monday 2020-12-28 -> within upper bound (2021-01-03)

        Assert.IsTrue(c.IsSatisfied);
        var items = c.ValidItems.Cast<DateOnly>().ToList();
        CollectionAssert.AreEqual(
            new[] { new DateOnly(2019,12,30), new DateOnly(2020,12,28) },
            items);
    }

    [TestMethod]
    public void Add_OutOfRangeWeek_ShouldInvalidate_AndStoreNothing()
    {
        var c = new BoundsValidatingWeekCollection(Lower, Upper);

        c.Add("2019-W01"); // way before lower bound

        Assert.IsFalse(c.IsSatisfied);
        Assert.AreEqual(0, c.ValidItems.Cast<object>().Count());
    }

    [TestMethod]
    public void Add_InvalidFormat_ShouldInvalidate_AndStoreNothing()
    {
        var c = new BoundsValidatingWeekCollection(Lower, Upper);

        c.Add("2020-25"); // missing 'W'

        Assert.IsFalse(c.IsSatisfied);
        Assert.AreEqual(0, c.ValidItems.Cast<object>().Count());
    }

    [TestMethod]
    public void Add_ValidThenInvalid_ShouldStopAcceptingFurtherItems()
    {
        var c = new BoundsValidatingWeekCollection(Lower, Upper);

        c.Add("2020-W10");
        c.Add("bad-format");
        c.Add("2020-W11");

        Assert.IsFalse(c.IsSatisfied);
        var items = c.ValidItems.Cast<DateOnly>().ToList();
        Assert.AreEqual(1, items.Count);
        Assert.AreEqual(new DateOnly(2020, 3, 2), items[0]); // Monday of ISO week 10, 2020
    }
}