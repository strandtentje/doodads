using Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Implementations.FieldType;

namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Implementations;
[TestClass]
public class ValidatingEmailCollectionTests
{
    [TestMethod]
    public void Add_ValidEmail_ShouldPass()
    {
        var c = new ValidatingEmailCollection();
        c.Add("alice@example.com");

        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual("alice@example.com", c.ValidItems.Cast<object>().Single());
    }

    [TestMethod]
    public void Add_InvalidEmail_ShouldFail()
    {
        var c = new ValidatingEmailCollection();
        c.Add("not-an-email");

        Assert.IsFalse(c.IsSatisfied);
        Assert.AreEqual(0, c.ValidItems.Cast<object>().Count());
    }
}