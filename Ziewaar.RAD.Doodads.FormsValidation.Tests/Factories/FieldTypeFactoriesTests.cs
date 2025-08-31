namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Factories;
[TestClass]
public class FieldTypeFactoriesTests
{
    [TestMethod]
    public void ValidatingColorCollectionFactory_CreatesColorValidator()
    {
        var coll = new ValidatingColorCollectionFactory().Create();
        coll.Add("#0Fa"); // valid short hex
        Assert.IsTrue(coll.IsSatisfied);
        Assert.AreEqual("#0Fa", coll.ValidItems.Cast<string>().Single());
    }

    [TestMethod]
    public void ValidatingDateOnlyCollectionFactory_CreatesDateValidator()
    {
        var coll = new ValidatingDateOnlyCollectionFactory().Create();
        coll.Add("2024-02-29");
        Assert.IsTrue(coll.IsSatisfied);
        Assert.AreEqual(new DateOnly(2024, 2, 29), coll.ValidItems.Cast<DateOnly>().Single());
    }

    [TestMethod]
    public void ValidatingDateTimeCollectionFactory_CreatesDateTimeValidator()
    {
        var coll = new ValidatingDateTimeCollectionFactory().Create();
        coll.Add("2024-02-29T12:34:56");
        Assert.IsTrue(coll.IsSatisfied);
        Assert.AreEqual(new DateTime(2024, 2, 29, 12, 34, 56), coll.ValidItems.Cast<DateTime>().Single());
    }

    [TestMethod]
    public void ValidatingEmailCollectionFactory_CreatesEmailValidator()
    {
        var coll = new ValidatingEmailCollectionFactory().Create();
        coll.Add("alice@example.com");
        Assert.IsTrue(coll.IsSatisfied);
        Assert.AreEqual("alice@example.com", coll.ValidItems.Cast<object>().Single());
    }

    [TestMethod]
    public void ValidatingFileCollectionFactory_CreatesFileValidator()
    {
        var coll = new ValidatingFileCollectionFactory().Create();
        using var ms = new MemoryStream();
        coll.Add(ms);
        Assert.IsTrue(coll.IsSatisfied);
        Assert.AreSame(ms, coll.ValidItems.Cast<object>().Single());
    }

    [TestMethod]
    public void ValidatingMonthCollectionFactory_CreatesMonthValidator_NormalizesToFirstOfMonth()
    {
        var coll = new ValidatingMonthCollectionFactory().Create();
        coll.Add("2024-07-31"); // lenient month impl expected
        Assert.IsTrue(coll.IsSatisfied);
        Assert.AreEqual(new DateOnly(2024, 7, 1), coll.ValidItems.Cast<DateOnly>().Single());
    }

    [TestMethod]
    public void ValidatingNumberCollectionFactory_CreatesNumberValidator()
    {
        var coll = new ValidatingNumberCollectionFactory().Create();
        coll.Add("1234.5");
        Assert.IsTrue(coll.IsSatisfied);
        Assert.AreEqual(1234.5m, coll.ValidItems.Cast<decimal>().Single());
    }

    [TestMethod]
    public void ValidatingTimeCollectionFactory_CreatesTimeValidator()
    {
        var coll = new ValidatingTimeCollectionFactory().Create();
        coll.Add("09:15");
        // NOTE: if ValidatingTimeCollection.IsSatisfied is not initialized to true in the implementation,
        // this will fail and surface the bug.
        Assert.IsTrue(coll.IsSatisfied, "ValidatingTimeCollection.IsSatisfied likely needs to default to true.");
        Assert.AreEqual(new TimeOnly(9, 15), coll.ValidItems.Cast<TimeOnly>().Single());
    }

    [TestMethod]
    public void ValidatingWeekCollectionFactory_CreatesWeekValidator()
    {
        var coll = new ValidatingWeekCollectionFactory().Create();
        coll.Add("2020-W25");
        Assert.IsTrue(coll.IsSatisfied);
        Assert.AreEqual(new DateOnly(2020, 6, 15), coll.ValidItems.Cast<DateOnly>().Single()); // Monday
    }
}