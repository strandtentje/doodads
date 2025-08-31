namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Implementations;
[TestClass]
public class BoundsValidatingMonthCollectionTests
{
    private readonly DateOnly Lower = new DateOnly(2020, 1, 1);
    private readonly DateOnly Upper = new DateOnly(2020, 12, 31);

    [TestMethod]
    public void Add_DateOnlyWithinBounds_ShouldBeValid()
    {
        var c = new BoundsValidatingMonthCollection(Lower, Upper);

        var d = new DateOnly(2020, 6, 15);
        c.Add(d, out var _);

        Assert.IsTrue(c.IsSatisfied);
        var stored = c.ValidItems.Cast<DateOnly>().Single();
        Assert.AreEqual(d, stored);
    }

    [TestMethod]
    public void Add_DateTimeWithinBounds_ShouldConvertToDateOnlyAndBeValid()
    {
        var c = new BoundsValidatingMonthCollection(Lower, Upper);

        var dt = new DateTime(2020, 5, 10, 12, 34, 56);
        c.Add(dt, out var _);

        Assert.IsTrue(c.IsSatisfied);
        var stored = c.ValidItems.Cast<DateOnly>().Single();
        Assert.AreEqual(new DateOnly(2020, 5, 10), stored);
    }

    [TestMethod]
    public void Add_StringYearMonthWithinBounds_ShouldParseAsFirstOfMonth()
    {
        var c = new BoundsValidatingMonthCollection(Lower, Upper);

        c.Add("2020-07", out var _); // becomes "2020-07-01" via $"{value}-01"

        Assert.IsTrue(c.IsSatisfied);
        var stored = c.ValidItems.Cast<DateOnly>().Single();
        Assert.AreEqual(new DateOnly(2020, 7, 1), stored);
    }

    [TestMethod]
    public void Add_OutOfRangeMonthString_ShouldInvalidate_AndStoreNothing()
    {
        var c = new BoundsValidatingMonthCollection(Lower, Upper);

        c.Add("2021-01", out var _);

        Assert.IsFalse(c.IsSatisfied);
        Assert.AreEqual(0, c.ValidItems.Cast<object>().Count());
    }

    [TestMethod]
    public void Add_InvalidString_ShouldInvalidate_AndStoreNothing()
    {
        var c = new BoundsValidatingMonthCollection(Lower, Upper);

        c.Add("not-a-month", out var _);

        Assert.IsFalse(c.IsSatisfied);
        Assert.AreEqual(0, c.ValidItems.Cast<object>().Count());
    }

    [TestMethod]
    public void Add_ValidThenInvalid_ShouldStopAcceptingFurtherItems()
    {
        var c = new BoundsValidatingMonthCollection(Lower, Upper);

        c.Add(new DateOnly(2020, 3, 10), out var _); // valid
        c.Add("nope", out var _);                    // invalid -> IsSatisfied false
        c.Add("2020-04", out var _);                 // would be valid, but should be ignored

        Assert.IsFalse(c.IsSatisfied);
        var items = c.ValidItems.Cast<DateOnly>().ToList();
        Assert.AreEqual(1, items.Count);
        Assert.AreEqual(new DateOnly(2020, 3, 10), items[0]);
    }

    [TestMethod]
    public void Add_ExactlyAtLowerBoundMonthString_ShouldBeValid()
    {
        var c = new BoundsValidatingMonthCollection(Lower, Upper);

        c.Add("2020-01"); // parses to 2020-01-01

        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual(new DateOnly(2020, 1, 1), c.ValidItems.Cast<DateOnly>().Single());
    }

    [TestMethod]
    public void Add_ExactlyAtUpperBoundMonthString_ShouldBeValid()
    {
        var c = new BoundsValidatingMonthCollection(Lower, Upper);

        c.Add("2020-12"); // parses to 2020-12-01 which is <= 2020-12-31

        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual(new DateOnly(2020, 12, 1), c.ValidItems.Cast<DateOnly>().Single());
    }
}