namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Factories;
[TestClass]
public class PatternValidationCollectionFactoryTests
{
    [TestMethod]
    public void Create_PatternValidator_MatchesWholeString()
    {
        var factory = new PatternValidationCollectionFactory(@"\d{3}");
        var coll = factory.Create();

        coll.Add("123");
        Assert.IsTrue(coll.IsSatisfied);
        var items = coll.ValidItems.Cast<object>().ToList();
        Assert.AreEqual(1, items.Count);
        Assert.AreEqual("123", items[0]);

        coll = factory.Create();
        coll.Add("abc123def"); // factory anchors the pattern with ^...$ so this should fail
        Assert.IsFalse(coll.IsSatisfied);
    }
}