using Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Factories.Bounding;

namespace Ziewaar.RAD.Doodads.FormsValidation.Tests;
[TestClass]
public class BoundsValidatingDateOnlyFactoryStrictTests
{
    [TestMethod]
    public void Picks_TightestWindow_And_Is_Inclusive()
    {
        // Lower bounds choose Max => 2020-03-01
        // Upper bounds choose Min => 2020-10-31
        var f = new BoundsValidatingDateOnlyCollectionFactory(
            new[] { "2020-01-01", "2020-03-01" },
            new[] { "2020-12-31", "2020-10-31" }
        );

        var c = f.Create();
        c.Add("2020-03-01"); // on lower bound
        Assert.IsTrue(c.IsSatisfied);

        c = f.Create();
        c.Add("2020-10-31"); // on upper bound
        Assert.IsTrue(c.IsSatisfied);

        c = f.Create();
        c.Add("2020-02-29"); // below lower
        Assert.IsFalse(c.IsSatisfied);

        c = f.Create();
        c.Add("2020-11-01"); // above upper
        Assert.IsFalse(c.IsSatisfied);
    }

    [TestMethod]
    public void Rejects_BadBoundFormats_AtConstruction()
    {
        // Not zero-padded, wrong separators, or impossible dates should throw.
        Assert.ThrowsExactly<FormatException>(() =>
            new BoundsValidatingDateOnlyCollectionFactory(new[] { "2020-7-01" }, new[] { "2020-12-31" }));

        Assert.ThrowsExactly<FormatException>(() =>
            new BoundsValidatingDateOnlyCollectionFactory(new[] { "2020/07/01" }, new[] { "2020-12-31" }));

        Assert.ThrowsExactly<FormatException>(() =>
            new BoundsValidatingDateOnlyCollectionFactory(new[] { "2020-02-30" }, new[] { "2020-12-31" }));
    }

    [TestMethod]
    public void EmptyArrays_DefaultToMinMax_DoNotThrow_AndValidate()
    {
        var f = new BoundsValidatingDateOnlyCollectionFactory(Array.Empty<string>(), Array.Empty<string>());
        var c = f.Create();

        c.Add("0001-01-01");
        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual(new DateOnly(1,1,1), c.ValidItems.Cast<DateOnly>().Single());
    }
}