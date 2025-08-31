namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Implementations;
[TestClass]
public class ValidatingMonthCollectionTests
{
    [TestMethod]
    public void Add_DateOnly_NormalizesToFirstOfMonth()
    {
        var c = new ValidatingMonthCollection();
        c.Add(new DateOnly(2024, 7, 15));

        Assert.IsTrue(c.IsSatisfied);
        var stored = c.ValidItems.Cast<DateOnly>().Single();
        Assert.AreEqual(new DateOnly(2024, 7, 1), stored);
    }

    [TestMethod]
    public void Add_DateTime_NormalizesToFirstOfMonth()
    {
        var c = new ValidatingMonthCollection();
        c.Add(new DateTime(2024, 2, 29, 18, 0, 0));

        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual(new DateOnly(2024, 2, 1), c.ValidItems.Cast<DateOnly>().Single());
    }

    [TestMethod]
    public void Add_String_YearDashMonth()
    {
        var c = new ValidatingMonthCollection();
        c.Add("2024-07");

        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual(new DateOnly(2024, 7, 1), c.ValidItems.Cast<DateOnly>().Single());
    }

    [TestMethod]
    public void Add_String_YearSlashMonth()
    {
        var c = new ValidatingMonthCollection();
        c.Add("2024/07");

        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual(new DateOnly(2024, 7, 1), c.ValidItems.Cast<DateOnly>().Single());
    }

    [TestMethod]
    public void Add_String_YearDashSingleDigitMonth()
    {
        var c = new ValidatingMonthCollection();
        c.Add("2024-7");

        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual(new DateOnly(2024, 7, 1), c.ValidItems.Cast<DateOnly>().Single());
    }

    [TestMethod]
    public void Add_String_FullDate_IsoLike()
    {
        var c = new ValidatingMonthCollection();
        c.Add("2024-07-31");

        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual(new DateOnly(2024, 7, 1), c.ValidItems.Cast<DateOnly>().Single());
    }

    [TestMethod]
    public void Add_InvalidString_ShouldInvalidate()
    {
        var c = new ValidatingMonthCollection();
        c.Add("not-a-month");

        Assert.IsFalse(c.IsSatisfied);
        Assert.AreEqual(0, c.ValidItems.Cast<object>().Count());
    }

    [TestMethod]
    public void Add_ValidThenInvalid_ShouldStopAccepting()
    {
        var c = new ValidatingMonthCollection();
        c.Add("2024-06");
        c.Add("nope");
        c.Add("2024-07"); // ignored

        Assert.IsFalse(c.IsSatisfied);
        var items = c.ValidItems.Cast<DateOnly>().ToList();
        CollectionAssert.AreEqual(new[] { new DateOnly(2024, 6, 1) }, items);
    }
}