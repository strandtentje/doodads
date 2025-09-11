using Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Implementations;

namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Implementations;
[TestClass]
public class LengthValidatorCollectionTests
{
    [TestMethod]
    public void Add_WithinRange_ShouldPass()
    {
        var c = new LengthValidatorCollection(minLength: 2, maxLength: 5);
        c.Add("hey");

        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual("hey", c.ValidItems.Cast<string>().Single());
    }

    [TestMethod]
    public void Add_TooShort_ShouldFail()
    {
        var c = new LengthValidatorCollection(2, 10);
        c.Add("a");

        Assert.IsFalse(c.IsSatisfied);
        Assert.AreEqual(0, c.ValidItems.Cast<object>().Count());
    }

    [TestMethod]
    public void Add_TooLong_ShouldFail()
    {
        var c = new LengthValidatorCollection(1, 3);
        c.Add("toolong");

        Assert.IsFalse(c.IsSatisfied);
        Assert.AreEqual(0, c.ValidItems.Cast<object>().Count());
    }
}