namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Implementations;
[TestClass]
public class BoundsValidatingDateCollectionTests
{
    private readonly DateOnly Lower = new(2020, 1, 1);
    private readonly DateOnly Upper = new(2020, 12, 31);
    [TestMethod]
    public void Add_DateOnlyWithinBounds_ShouldBeValid()
    {
        var collection = new BoundsValidatingDateCollection(Lower, Upper);

        collection.Add(new DateOnly(2020, 6, 15), out var _);

        Assert.IsTrue(collection.IsSatisfied);
        Assert.AreEqual(1, collection.ValidItems.Cast<object>().Count());
    }
    [TestMethod]
    public void Add_DateTimeWithinBounds_ShouldBeValid()
    {
        var collection = new BoundsValidatingDateCollection(Lower, Upper);

        collection.Add(new DateTime(2020, 5, 1), out var _);

        Assert.IsTrue(collection.IsSatisfied);
        Assert.AreEqual(new DateOnly(2020, 5, 1), collection.ValidItems.Cast<DateOnly>().Single());
    }
    [TestMethod]
    public void Add_StringParsableWithinBounds_ShouldBeValid()
    {
        var collection = new BoundsValidatingDateCollection(Lower, Upper);

        collection.Add("2020-07-20", out var _);

        Assert.IsTrue(collection.IsSatisfied);
        Assert.AreEqual(new DateOnly(2020, 7, 20), collection.ValidItems.Cast<DateOnly>().Single());
    }
    [TestMethod]
    public void Add_OutOfRangeDate_ShouldInvalidate()
    {
        var collection = new BoundsValidatingDateCollection(Lower, Upper);

        collection.Add(new DateOnly(2021, 1, 1), out var _);

        Assert.IsFalse(collection.IsSatisfied);
        Assert.AreEqual(0, collection.ValidItems.Cast<object>().Count());
    }
    [TestMethod]
    public void Add_InvalidString_ShouldInvalidate()
    {
        var collection = new BoundsValidatingDateCollection(Lower, Upper);

        collection.Add("not-a-date", out var _);

        Assert.IsFalse(collection.IsSatisfied);
        Assert.AreEqual(0, collection.ValidItems.Cast<object>().Count());
    }
    [TestMethod]
    public void Add_ValidThenInvalid_ShouldStopAccepting()
    {
        var collection = new BoundsValidatingDateCollection(Lower, Upper);

        collection.Add(new DateOnly(2020, 3, 10), out var _); // valid
        collection.Add("not-a-date", out var _); // invalid
        collection.Add(new DateOnly(2020, 4, 10), out var _); // would be valid, but IsSatisfied already false

        Assert.IsFalse(collection.IsSatisfied);
        Assert.AreEqual(1, collection.ValidItems.Cast<object>().Count()); // only first one kept
    }
}