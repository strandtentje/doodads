namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Factories;
[TestClass]
public class TypeValidatingCollectionFactoryTests2
{
    [TestMethod]
    public void Input_Color_ProducesColorValidator()
    {
        var f = new TypeValidatingCollectionFactory(fieldTag: "input", fieldType: "color");
        var c = f.Create();

        c.Add("#00FFA0");
        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual("#00FFA0", c.ValidItems.Cast<string>().Single());
    }

    [TestMethod]
    public void Input_Date_ProducesDateValidator()
    {
        var f = new TypeValidatingCollectionFactory("input", "date");
        var c = f.Create();

        c.Add("2020-02-29");
        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual(new DateOnly(2020, 2, 29), c.ValidItems.Cast<DateOnly>().Single());
    }

    [TestMethod]
    public void Input_DateTimeLocal_ProducesDateTimeValidator()
    {
        var f = new TypeValidatingCollectionFactory("input", "datetime-local");
        var c = f.Create();

        c.Add("2020-12-31T23:59:59");
        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual(new DateTime(2020, 12, 31, 23, 59, 59), c.ValidItems.Cast<DateTime>().Single());
    }

    [TestMethod]
    public void Input_Email_ProducesEmailValidator()
    {
        var f = new TypeValidatingCollectionFactory("input", "email");
        var c = f.Create();

        c.Add("alice@example.com");
        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual("alice@example.com", c.ValidItems.Cast<object>().Single());
    }

    [TestMethod]
    public void Input_Month_ProducesMonthValidator_NormalizesToFirst()
    {
        var f = new TypeValidatingCollectionFactory("input", "month");
        var c = f.Create();

        c.Add("2024-07-31");
        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual(new DateOnly(2024, 7, 1), c.ValidItems.Cast<DateOnly>().Single());
    }

    [TestMethod]
    public void Input_Number_ProducesNumberValidator()
    {
        var f = new TypeValidatingCollectionFactory("input", "number");
        var c = f.Create();

        c.Add("42");
        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual(42m, c.ValidItems.Cast<decimal>().Single());
    }

    [TestMethod]
    public void Input_Range_AlsoProducesNumberValidator()
    {
        var f = new TypeValidatingCollectionFactory("input", "range");
        var c = f.Create();

        c.Add("3");
        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual(3m, c.ValidItems.Cast<decimal>().Single());
    }

    [TestMethod]
    public void Input_Time_ProducesTimeValidator()
    {
        var f = new TypeValidatingCollectionFactory("input", "time");
        var c = f.Create();

        c.Add("08:15");
        Assert.IsTrue(c.IsSatisfied); // if this fails, ensure ValidatingTimeCollection.IsSatisfied defaults to true
        Assert.AreEqual(new TimeOnly(8, 15), c.ValidItems.Cast<TimeOnly>().Single());
    }

    [TestMethod]
    public void Input_Week_ProducesWeekValidator()
    {
        var f = new TypeValidatingCollectionFactory("input", "week");
        var c = f.Create();

        c.Add("2020-W25");
        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual(new DateOnly(2020, 6, 15), c.ValidItems.Cast<DateOnly>().Single()); // Monday
    }

    [TestMethod]
    public void NonInput_Tag_ProducesNonValidating()
    {
        var f = new TypeValidatingCollectionFactory("select", "number");
        var c = f.Create();

        c.Add("anything");
        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual("anything", c.ValidItems.Cast<object>().Single());
    }

    [TestMethod]
    public void Input_UnknownType_ProducesNonValidating()
    {
        var f = new TypeValidatingCollectionFactory("input", "unknown-type");
        var c = f.Create();

        c.Add("whatever");
        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual("whatever", c.ValidItems.Cast<object>().Single());
    }
}