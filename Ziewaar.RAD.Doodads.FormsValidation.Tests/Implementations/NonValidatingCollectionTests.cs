namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Implementations;
[TestClass]
public class NonValidatingCollectionTests
{
    [TestMethod]
    public void Add_Anything_ShouldAlwaysSucceed()
    {
        var c = new NonValidatingCollection();
        c.Add(1);
        c.Add("x");
        c.Add(new object());

        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual(3, c.ValidItems.Cast<object>().Count());
    }
}