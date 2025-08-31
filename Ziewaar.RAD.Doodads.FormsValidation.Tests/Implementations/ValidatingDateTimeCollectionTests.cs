using Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Implementations.FieldType;

namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Implementations;
[TestClass]
public class ValidatingDateTimeCollectionTests
{
    [TestMethod]
    public void Add_DateTime_ShouldPass()
    {
        var c = new ValidatingDateTimeCollection();
        var dt = new DateTime(2020, 12, 31, 23, 59, 59);

        c.Add(dt);

        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual(dt, c.ValidItems.Cast<DateTime>().Single());
    }

    [TestMethod]
    public void Add_StringParsable_ShouldPass()
    {
        var c = new ValidatingDateTimeCollection();
        c.Add("2021-01-01T08:30:00");

        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual(new DateTime(2021, 1, 1, 8, 30, 0), c.ValidItems.Cast<DateTime>().Single());
    }

    [TestMethod]
    public void Add_Invalid_ShouldFail()
    {
        var c = new ValidatingDateTimeCollection();
        c.Add("whoops");

        Assert.IsFalse(c.IsSatisfied);
        Assert.AreEqual(0, c.ValidItems.Cast<object>().Count());
    }
}