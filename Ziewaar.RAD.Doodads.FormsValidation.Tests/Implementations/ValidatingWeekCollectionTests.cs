using Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Implementations.FieldType;

namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Implementations;
[TestClass]
public class ValidatingWeekCollectionTests
{
    [TestMethod]
    public void Add_DateOnly_ShouldStoreAsIs()
    {
        var c = new ValidatingWeekCollection();
        var d = new DateOnly(2023, 5, 1);

        c.Add(d);

        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual(d, c.ValidItems.Cast<DateOnly>().Single());
    }

    [TestMethod]
    public void Add_WeekString_ShouldStoreWeekStartMonday()
    {
        var c = new ValidatingWeekCollection();

        c.Add("2020-W25");

        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual(new DateOnly(2020, 6, 15), c.ValidItems.Cast<DateOnly>().Single());
    }

    [TestMethod]
    public void Add_InvalidWeekString_ShouldFail()
    {
        var c = new ValidatingWeekCollection();

        c.Add("2020-25"); // wrong format

        Assert.IsFalse(c.IsSatisfied);
        Assert.AreEqual(0, c.ValidItems.Cast<object>().Count());
    }
}