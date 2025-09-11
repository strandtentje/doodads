using Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Implementations.FieldType;

namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Implementations;
[TestClass]
public class ValidatingDateCollectionTests
{
    [TestMethod]
    public void Add_DateOnly_ShouldPass()
    {
        var c = new ValidatingDateCollection();
        var d = new DateOnly(2024, 2, 29);

        c.Add(d);

        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual(d, c.ValidItems.Cast<DateOnly>().Single());
    }

    [TestMethod]
    public void Add_IsoStringParsable_ShouldPass()
    {
        var c = new ValidatingDateCollection();
        c.Add("2023-10-05");

        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual(new DateOnly(2023, 10, 5), c.ValidItems.Cast<DateOnly>().Single());
    }

    [TestMethod]
    public void Add_InvalidString_ShouldFail()
    {
        var c = new ValidatingDateCollection();
        c.Add("not-a-date");

        Assert.IsFalse(c.IsSatisfied);
        Assert.AreEqual(0, c.ValidItems.Cast<object>().Count());
    }
}