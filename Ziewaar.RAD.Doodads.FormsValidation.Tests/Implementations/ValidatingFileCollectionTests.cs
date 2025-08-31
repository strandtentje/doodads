namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Implementations;
[TestClass]
public class ValidatingFileCollectionTests
{
    [TestMethod]
    public void Add_Stream_ShouldPass()
    {
        var c = new ValidatingFileCollection();
        using var ms = new MemoryStream(new byte[] { 1, 2, 3 });

        c.Add(ms);

        Assert.IsTrue(c.IsSatisfied);
        Assert.AreSame(ms, c.ValidItems.Cast<object>().Single());
    }

    [TestMethod]
    public void Add_NonStream_ShouldFail()
    {
        var c = new ValidatingFileCollection();
        c.Add("not a stream");

        Assert.IsFalse(c.IsSatisfied);
        Assert.AreEqual(0, c.ValidItems.Cast<object>().Count());
    }
}