namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Factories;
[TestClass]
public class OperatorsWithFactoriesCompositionTests
{
    [TestMethod]
    public void AllValid_TypeNumber_WithBounds_AcceptsInside_RejectsOutside()
    {
        var typeF = new TypeValidatingCollectionFactory("input", "number");
        var boundsF = new BoundsValidatingCollectionFactory("number", new[] { "-5" }, new[] { "5" });
        var allF = new AllValidCollectionsFactory(typeF, boundsF);

        var c = allF.Create();
        c.Add("3");
        Assert.IsTrue(c.IsSatisfied);
        CollectionAssert.AreEqual(new object[] { 3m }, c.ValidItems.Cast<object>().ToArray());

        c = allF.Create();
        c.Add("6"); // fails bounds
        Assert.IsFalse(c.IsSatisfied);
    }

    [TestMethod]
    public void AllValid_TypeMonth_WithBounds_NormalizesAndChecksRange()
    {
        var typeF = new TypeValidatingCollectionFactory("input", "month");
        var boundsF = new BoundsValidatingCollectionFactory("month", new[] { "2020-01-01" }, new[] { "2020-12-31" });
        var allF = new AllValidCollectionsFactory(typeF, boundsF);

        var c = allF.Create();
        c.Add("2020-07-31");
        Assert.IsTrue(c.IsSatisfied);
        CollectionAssert.AreEqual(new object[] { new DateOnly(2020, 7, 1) }, c.ValidItems.Cast<object>().ToArray());

        c = allF.Create();
        c.Add("2021-01-15");
        Assert.IsFalse(c.IsSatisfied);
    }

    [TestMethod]
    public void AnyValid_TypeNumber_Or_TypeColor_AcceptsEither()
    {
        var numberType = new TypeValidatingCollectionFactory("input", "number");
        var colorType = new TypeValidatingCollectionFactory("input", "color");
        var anyF = new AnyValidCollectionFactory(numberType, colorType);

        var c = anyF.Create();
        c.Add("123");
        Assert.IsTrue(c.IsSatisfied);
        CollectionAssert.AreEqual(new object[] { 123m }, c.ValidItems.Cast<object>().ToArray());

        c = anyF.Create();
        c.Add("#abcdef");
        Assert.IsTrue(c.IsSatisfied);
        CollectionAssert.AreEqual(new object[] { "#abcdef" }, c.ValidItems.Cast<object>().ToArray());

        c = anyF.Create();
        c.Add("not-a-number-or-color");
        Assert.IsFalse(c.IsSatisfied);
    }
}