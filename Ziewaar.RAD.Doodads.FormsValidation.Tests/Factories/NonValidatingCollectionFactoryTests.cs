using Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Factories.Operators;

namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Factories;
[TestClass]
public class NonValidatingCollectionFactoryTests
{
    [TestMethod]
    public void Create_Returns_NonValidatingCollection_StoresEverything()
    {
        var factory = new NonValidatingCollectionFactory();
        var coll = factory.Create();

        coll.Add(1);
        coll.Add("x");
        coll.Add(new object());

        Assert.IsTrue(coll.IsSatisfied);
        Assert.AreEqual(3, coll.ValidItems.Cast<object>().Count());
    }
}