using Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Factories.Bounding;
using Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Implementations.FieldType;

namespace Ziewaar.RAD.Doodads.FormsValidation.Tests;
[TestClass]
public class HtmlTimeParsingTests
{
    [TestMethod]
    public void ValidatingTime_StrictFormats_AcceptAndReject()
    {
        var c = new ValidatingTimeCollection();
        c.Add("09:07");
        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual(new TimeOnly(9, 7), c.ValidItems.Cast<TimeOnly>().Single());

        c = new ValidatingTimeCollection();
        c.Add("09:07:03.1234567");
        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual(new TimeOnly(9, 7, 3, 123), c.ValidItems.Cast<TimeOnly>().Single().TruncateMilliseconds(3));

        // Reject: space instead of colon, single-digit hour, out of range
        c = new ValidatingTimeCollection();
        c.Add("9:07");         // HTML spec uses zero-padded hours
        Assert.IsFalse(c.IsSatisfied);

        c = new ValidatingTimeCollection();
        c.Add("24:00");
        Assert.IsFalse(c.IsSatisfied);
    }

    [TestMethod]
    public void Bounds_Time_StrictFormats_OnBounds()
    {
        var f = new BoundsValidatingTimeCollectionFactory(new[] { "09:00" }, new[] { "17:30:00" });
        var c = f.Create();

        c.Add("09:00");
        Assert.IsTrue(c.IsSatisfied);

        c = f.Create();
        c.Add("17:30:00");
        Assert.IsTrue(c.IsSatisfied);

        c = f.Create();
        c.Add("08:59");
        Assert.IsFalse(c.IsSatisfied);

        Assert.ThrowsExactly<FormatException>(() =>
            new BoundsValidatingTimeCollectionFactory(new[] { "9:00" }, new[] { "17:30" })); // not zero-padded
    }
}