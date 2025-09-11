using Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Implementations.FieldType;

namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Implementations;
[TestClass]
public class ValidatingNumberCollectionTests
{
    [TestMethod]
    public void Add_DecimalStringInvariant_ShouldPass()
    {
        var c = new ValidatingNumberCollection();

        c.Add("1234.50");

        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual(1234.50m, c.ValidItems.Cast<decimal>().Single());
    }

    [TestMethod]
    public void Add_InvalidNumber_ShouldFail()
    {
        var c = new ValidatingNumberCollection();

        c.Add("1.234.50"); // culture-specific format - should fail with InvariantCulture

        Assert.IsFalse(c.IsSatisfied);
        Assert.AreEqual(0, c.ValidItems.Cast<object>().Count());
    }
}