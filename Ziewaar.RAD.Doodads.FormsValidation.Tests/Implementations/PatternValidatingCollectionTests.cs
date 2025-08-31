using System.Text.RegularExpressions;

namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Implementations;
[TestClass]
public class PatternValidatingCollectionTests
{
    [TestMethod]
    public void Add_MatchingPattern_ShouldPass()
    {
        var c = new PatternValidatingCollection(new Regex(@"^\d{3}-\d{2}-\d{4}$"));

        c.Add("123-45-6789");

        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual("123-45-6789", c.ValidItems.Cast<object>().Single());
    }

    [TestMethod]
    public void Add_NotMatchingPattern_ShouldFail()
    {
        var c = new PatternValidatingCollection(new Regex(@"^\d{3}-\d{2}-\d{4}$"));

        c.Add("ABC");

        Assert.IsFalse(c.IsSatisfied);
        Assert.AreEqual(0, c.ValidItems.Cast<object>().Count());
    }
}