using Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Factories.Bounding;
using Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Factories.Composite;

namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Factories;
[TestClass]
public class BoundsValidatingCollectionFactoryTests
{
    [TestMethod]
    public void Date_Bounds_ProducesBoundedDateValidator()
    {
        var f = new BoundsValidatingDateOnlyCollectionFactory(new[] { "2020-01-01" }, new[] { "2020-12-31" });
        var c = f.Create();

        c.Add("2020-06-15");
        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual(new DateOnly(2020, 6, 15), c.ValidItems.Cast<DateOnly>().Single());

        c = f.Create();
        c.Add("2019-12-31");
        Assert.IsFalse(c.IsSatisfied);
    }

    [TestMethod]
    public void Date_BoundsFactoryWrapper_SelectsCorrectFactory()
    {
        var wrapper = new BoundsValidatingCollectionFactory("date", new[] { "2020-01-01" }, new[] { "2020-12-31" });
        var c = wrapper.Create();

        c.Add("2020-02-02");
        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual(new DateOnly(2020, 2, 2), c.ValidItems.Cast<DateOnly>().Single());
    }

    [TestMethod]
    public void Date_InvalidBounds_Throws()
    {
        Assert.ThrowsExactly<FormatException>(() =>
            new BoundsValidatingDateOnlyCollectionFactory(new[] { "2020-13-01" }, new[] { "2020-12-31" }));
        Assert.ThrowsExactly<FormatException>(() =>
            new BoundsValidatingCollectionFactory("date", new[] { "bad" }, new[] { "2020-12-31" }));
    }

    [TestMethod]
    public void DateTimeLocal_Bounds_ProducesBoundedDateTimeValidator()
    {
        var f = new BoundsValidatingDateTimeCollectionFactory(
            new[] { "2020-01-01T00:00:00" }, new[] { "2020-12-31T23:59:59" });
        var c = f.Create();

        c.Add("2020-12-31T23:59:59");
        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual(new DateTime(2020, 12, 31, 23, 59, 59), c.ValidItems.Cast<DateTime>().Single());

        c = f.Create();
        c.Add("2021-01-01T00:00:00");
        Assert.IsFalse(c.IsSatisfied);
    }

    [TestMethod]
    public void DateTimeLocal_InvalidBounds_Throws()
    {
        Assert.ThrowsExactly<FormatException>(() =>
            new BoundsValidatingDateTimeCollectionFactory(new[] { "2020-01-01T99:00:00" }, new[] { "2020-12-31T23:59:59" }));
        Assert.ThrowsExactly<FormatException>(() =>
            new BoundsValidatingCollectionFactory("datetime-local", new[] { "nope" }, new[] { "2020-12-31T23:59:59" }));
    }

    [TestMethod]
    public void Month_Bounds_ProducesBoundedMonthValidator()
    {
        // Factory parses DateOnly for bounds; must be full dates
        var f = new BoundsValidatingMonthCollectionFactory(new[] { "2020-01-01" }, new[] { "2020-12-31" });
        var c = f.Create();

        c.Add("2020-07"); // validator should normalize to 2020-07-01
        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual(new DateOnly(2020, 7, 1), c.ValidItems.Cast<DateOnly>().Single());

        c = f.Create();
        c.Add("2021-01");
        Assert.IsFalse(c.IsSatisfied);
    }

    [TestMethod]
    public void Month_InvalidBounds_Throws()
    {
        Assert.ThrowsExactly<FormatException>(() =>
            new BoundsValidatingMonthCollectionFactory(new[] { "2020-13-01" }, new[] { "2020-12-31" }));
        // Passing "yyyy-MM" (without day) will also throw because DateOnly.Parse expects a date:
    }

    [TestMethod]
    public void Number_Bounds_ProducesBoundedNumberValidator()
    {
        var f = new BoundsValidatingNumberCollectionFactory(new[] { "-5" }, new[] { "5" });
        var c = f.Create();

        c.Add("3");
        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual(3m, c.ValidItems.Cast<decimal>().Single());

        c = f.Create();
        c.Add("6");
        Assert.IsFalse(c.IsSatisfied);
    }

    [TestMethod]
    public void Number_InvalidBounds_Throws()
    {
        Assert.ThrowsExactly<FormatException>(() =>
            new BoundsValidatingNumberCollectionFactory(new[] { "abc" }, new[] { "5" }));
        Assert.ThrowsExactly<FormatException>(() =>
            new BoundsValidatingCollectionFactory("number", new[] { "-5" }, new[] { "oops" }));
    }

    [TestMethod]
    public void Time_Bounds_ProducesBoundedTimeValidator()
    {
        var f = new BoundsValidatingTimeCollectionFactory(new[] { "08:00" }, new[] { "17:30" });
        var c = f.Create();

        c.Add("08:00");
        Assert.IsTrue(c.IsSatisfied, "If failing, ensure ValidatingTimeCollection.IsSatisfied defaults to true.");
        Assert.AreEqual(new TimeOnly(8, 0), c.ValidItems.Cast<TimeOnly>().Single());

        c = f.Create();
        c.Add("07:59");
        Assert.IsFalse(c.IsSatisfied);
    }

    [TestMethod]
    public void Time_InvalidBounds_Throws()
    {
        Assert.ThrowsExactly<FormatException>(() =>
            new BoundsValidatingTimeCollectionFactory(new[] { "25:00" }, new[] { "17:30" }));
        Assert.ThrowsExactly<FormatException>(() =>
            new BoundsValidatingCollectionFactory("time", new[] { "08-00" }, new[] { "17:30" }));
    }

    [TestMethod]
    public void Week_Bounds_ProducesBoundedWeekValidator()
    {
        var f = new BoundsValidatingWeekCollectionFactory(new[] { "2020-W01" }, new[] { "2020-W53" });
        var c = f.Create();

        c.Add("2020-W25");
        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual(new DateOnly(2020, 6, 15), c.ValidItems.Cast<DateOnly>().Single());

        c = f.Create();
        c.Add("2019-W52");
        Assert.IsFalse(c.IsSatisfied);
    }

    [TestMethod]
    public void Week_InvalidBounds_Throws()
    {
        Assert.ThrowsExactly<FormatException>(() =>
            new BoundsValidatingWeekCollectionFactory(new[] { "2020-25" }, new[] { "2020-W53" })); // wrong format
        Assert.ThrowsExactly<FormatException>(() =>
            new BoundsValidatingCollectionFactory("week", new[] { "bad" }, new[] { "2020-W53" }));
    }

    [TestMethod]
    public void Bounds_Wrapper_UnknownType_YieldsNonValidating()
    {
        var wrapper = new BoundsValidatingCollectionFactory("unknown", Array.Empty<string>(), Array.Empty<string>());
        var c = wrapper.Create();

        c.Add("anything");
        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual("anything", c.ValidItems.Cast<object>().Single());
    }

    [TestMethod]
    public void Bounds_EmptyArrays_DefaultToMinMax_DoNotThrow()
    {
        var f = new BoundsValidatingDateOnlyCollectionFactory(Array.Empty<string>(), Array.Empty<string>());
        var c = f.Create();

        c.Add("0001-01-01");
        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual(new DateOnly(1, 1, 1), c.ValidItems.Cast<DateOnly>().Single());
    }
}