using Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Implementations.FieldType;

namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Implementations;
[TestClass]
public class ValidatingColorCollectionTests
{
    [TestMethod]
    public void Add_ValidShortHex_ShouldPass()
    {
        var c = new ValidatingColorCollection();
        c.Add("#0Fa");

        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual("#0Fa", c.ValidItems.Cast<string>().Single());
    }

    [TestMethod]
    public void Add_ValidLongHex_ShouldPass()
    {
        var c = new ValidatingColorCollection();
        c.Add("#00FFA0");

        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual("#00FFA0", c.ValidItems.Cast<string>().Single());
    }

    [TestMethod]
    public void Add_MissingHash_ShouldFail()
    {
        var c = new ValidatingColorCollection();
        c.Add("00FFA0");

        Assert.IsFalse(c.IsSatisfied);
        Assert.AreEqual(0, c.ValidItems.Cast<object>().Count());
    }

    [TestMethod]
    public void Add_NonString_ShouldFail()
    {
        var c = new ValidatingColorCollection();
        c.Add(123);

        Assert.IsFalse(c.IsSatisfied);
        Assert.AreEqual(0, c.ValidItems.Cast<object>().Count());
    }
}