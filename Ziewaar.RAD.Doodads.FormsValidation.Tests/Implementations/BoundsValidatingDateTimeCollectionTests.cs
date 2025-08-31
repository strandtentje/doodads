using Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Implementations.Bounding;

namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Implementations;
[TestClass]
public class BoundsValidatingDateTimeCollectionTests
{
    private readonly DateTime Lower = new DateTime(2020, 1, 1, 0, 0, 0);
    private readonly DateTime Upper = new DateTime(2020, 12, 31, 23, 59, 59);

    [TestMethod]
    public void Add_DateTimeWithinBounds_ShouldBeValid()
    {
        var c = new BoundsValidatingDateTimeCollection(Lower, Upper);

        var dt = new DateTime(2020, 5, 10, 12, 30, 0);
        c.Add(dt, out var _);

        Assert.IsTrue(c.IsSatisfied);
        var stored = c.ValidItems.Cast<DateTime>().Single();
        Assert.AreEqual(dt, stored);
    }

    [TestMethod]
    public void Add_DateOnlyWithinBounds_ShouldConvertToMidnightAndBeValid()
    {
        var c = new BoundsValidatingDateTimeCollection(Lower, Upper);

        var d = new DateOnly(2020, 7, 20);
        c.Add(d, out var _);

        Assert.IsTrue(c.IsSatisfied);
        var stored = c.ValidItems.Cast<DateTime>().Single();
        Assert.AreEqual(new DateTime(2020, 7, 20, 0, 0, 0), stored); // TimeOnly.MinValue
    }

    [TestMethod]
    public void Add_StringParsableWithinBounds_ShouldBeValid()
    {
        var c = new BoundsValidatingDateTimeCollection(Lower, Upper);

        c.Add("2020-03-15T08:45:00", out var _);

        Assert.IsTrue(c.IsSatisfied);
        var stored = c.ValidItems.Cast<DateTime>().Single();
        Assert.AreEqual(new DateTime(2020, 3, 15, 8, 45, 0), stored);
    }

    [TestMethod]
    public void Add_OutOfRangeDate_ShouldInvalidate_AndStoreNothing()
    {
        var c = new BoundsValidatingDateTimeCollection(Lower, Upper);

        c.Add(new DateTime(2021, 1, 1), out var _);

        Assert.IsFalse(c.IsSatisfied);
        Assert.AreEqual(0, c.ValidItems.Cast<object>().Count());
    }

    [TestMethod]
    public void Add_InvalidString_ShouldInvalidate_AndStoreNothing()
    {
        var c = new BoundsValidatingDateTimeCollection(Lower, Upper);

        c.Add("not-a-date", out var _);

        Assert.IsFalse(c.IsSatisfied);
        Assert.AreEqual(0, c.ValidItems.Cast<object>().Count());
    }

    [TestMethod]
    public void Add_ValidThenInvalid_ShouldStopAcceptingFurtherItems()
    {
        var c = new BoundsValidatingDateTimeCollection(Lower, Upper);

        c.Add(new DateTime(2020, 4, 1), out var _); // valid
        c.Add("nope", out var _);                   // invalid -> IsSatisfied false
        c.Add(new DateTime(2020, 5, 1), out var _); // would be valid, but should be ignored

        Assert.IsFalse(c.IsSatisfied);
        var items = c.ValidItems.Cast<DateTime>().ToList();
        Assert.AreEqual(1, items.Count);
        Assert.AreEqual(new DateTime(2020, 4, 1), items[0]);
    }

    [TestMethod]
    public void Add_ExactlyAtLowerBound_ShouldBeValid()
    {
        var c = new BoundsValidatingDateTimeCollection(Lower, Upper);

        c.Add(Lower, out var _);

        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual(Lower, c.ValidItems.Cast<DateTime>().Single());
    }

    [TestMethod]
    public void Add_ExactlyAtUpperBound_ShouldBeValid()
    {
        var c = new BoundsValidatingDateTimeCollection(Lower, Upper);

        c.Add(Upper, out var _);

        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual(Upper, c.ValidItems.Cast<DateTime>().Single());
    }
}