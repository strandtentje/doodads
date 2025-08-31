namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Factories;
[TestClass]
public class BoundsFactoriesTests
{
    [TestMethod]
    public void Bounds_DateOnly_EnforcesInclusiveBounds()
    {
        var factory = new BoundsValidatingDateOnlyCollectionFactory(
            new[] { "2020-01-01" }, new[] { "2020-12-31" });
        var coll = factory.Create();

        coll.Add("2020-06-15");
        Assert.IsTrue(coll.IsSatisfied);
        Assert.AreEqual(new DateOnly(2020, 6, 15), coll.ValidItems.Cast<DateOnly>().Single());

        coll = factory.Create();
        coll.Add("2019-12-31"); // below
        Assert.IsFalse(coll.IsSatisfied);
    }

    [TestMethod]
    public void Bounds_DateTime_EnforcesInclusiveBounds()
    {
        var factory = new BoundsValidatingDateTimeCollectionFactory(
            new[] { "2020-01-01T00:00:00" }, new[] { "2020-12-31T23:59:59" });
        var coll = factory.Create();

        coll.Add("2020-12-31T23:59:59");
        Assert.IsTrue(coll.IsSatisfied);
        Assert.AreEqual(new DateTime(2020, 12, 31, 23, 59, 59), coll.ValidItems.Cast<DateTime>().Single());

        coll = factory.Create();
        coll.Add("2021-01-01T00:00:00"); // above
        Assert.IsFalse(coll.IsSatisfied);
    }

    [TestMethod]
    public void Bounds_Month_EnforcesInclusiveBounds_FirstOfMonthNormalization()
    {
        var factory = new BoundsValidatingMonthCollectionFactory(
            new[] { "2020-01-01" }, new[] { "2020-12-31" });
        var coll = factory.Create();

        coll.Add("2020-06"); // expect 2020-06-01
        Assert.IsTrue(coll.IsSatisfied);
        Assert.AreEqual(new DateOnly(2020, 6, 1), coll.ValidItems.Cast<DateOnly>().Single());

        coll = factory.Create();
        coll.Add("2021-01"); // out of range
        Assert.IsFalse(coll.IsSatisfied);
    }

    [TestMethod]
    public void Bounds_Number_EnforcesInclusiveBounds()
    {
        var factory = new BoundsValidatingNumberCollectionFactory(
            new[] { "-10" }, new[] { "10" });
        var coll = factory.Create();

        coll.Add("10");
        Assert.IsTrue(coll.IsSatisfied);
        Assert.AreEqual(10m, coll.ValidItems.Cast<decimal>().Single());

        coll = factory.Create();
        coll.Add("10.0001");
        Assert.IsFalse(coll.IsSatisfied);
    }

    [TestMethod]
    public void Bounds_Time_EnforcesInclusiveBounds()
    {
        var factory = new BoundsValidatingTimeCollectionFactory(
            new[] { "08:00" }, new[] { "17:30" });
        var coll = factory.Create();

        coll.Add("08:00");
        // see earlier note about ValidatingTimeCollection default IsSatisfied
        Assert.IsTrue(coll.IsSatisfied, "ValidatingTimeCollection.IsSatisfied likely needs to default to true.");
        Assert.AreEqual(new TimeOnly(8, 0), coll.ValidItems.Cast<TimeOnly>().Single());

        coll = factory.Create();
        coll.Add("07:59");
        Assert.IsFalse(coll.IsSatisfied);
    }

    [TestMethod]
    public void Bounds_Week_EnforcesInclusiveBounds_WithWeekParsing()
    {
        var factory = new BoundsValidatingWeekCollectionFactory(
            new[] { "2020-W01" }, new[] { "2020-W53" });
        var coll = factory.Create();

        coll.Add("2020-W25");
        Assert.IsTrue(coll.IsSatisfied);
        Assert.AreEqual(new DateOnly(2020, 6, 15), coll.ValidItems.Cast<DateOnly>().Single());

        coll = factory.Create();
        coll.Add("2019-W52");
        Assert.IsFalse(coll.IsSatisfied);
    }
}