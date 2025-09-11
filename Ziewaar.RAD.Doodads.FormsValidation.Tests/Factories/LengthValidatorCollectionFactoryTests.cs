using Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Factories;

namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Factories;
[TestClass]
public class LengthValidatorCollectionFactoryTests
{
    [TestMethod]
    public void Create_LengthValidator_EnforcesBounds()
    {
        var factory = new LengthValidatorCollectionFactory(2, 5);
        var coll = factory.Create();

        coll.Add("ok");
        Assert.IsTrue(coll.IsSatisfied);
        CollectionAssert.AreEqual(new[] { "ok" }, coll.ValidItems.Cast<string>().ToArray());

        coll.Add("toolong"); // exceeds 5 => invalidate and do not add
        Assert.IsFalse(coll.IsSatisfied);
        // previously stored value should remain
        CollectionAssert.AreEqual(new[] { "ok" }, coll.ValidItems.Cast<string>().ToArray());
    }
}