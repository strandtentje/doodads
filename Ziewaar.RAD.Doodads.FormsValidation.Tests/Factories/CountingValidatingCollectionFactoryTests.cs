namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Factories;
[TestClass]
public class CountingValidatingCollectionFactoryTests
{
    [TestMethod]
    public void Create_CountingCollection_ReportsSatisfactionByCount()
    {
        var factory = new CountingValidatingCollectionFactory(1, 2);
        var coll = factory.Create();

        coll.Add("a");
        Assert.IsTrue(coll.IsSatisfied);

        coll.Add("b");
        Assert.IsTrue(coll.IsSatisfied);

        coll.Add("c");
        Assert.IsFalse(coll.IsSatisfied);
    }
}