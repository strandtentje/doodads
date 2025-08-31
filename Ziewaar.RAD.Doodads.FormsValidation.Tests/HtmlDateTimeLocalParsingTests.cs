namespace Ziewaar.RAD.Doodads.FormsValidation.Tests;
[TestClass]
public class HtmlDateTimeLocalParsingTests
{
    [TestMethod]
    public void ValidatingDateTime_StrictFormats_AcceptAndReject()
    {
        var c = new ValidatingDateTimeCollection();

        c.Add("2024-07-31T09:07");
        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual(new DateTime(2024, 7, 31, 9, 7, 0), c.ValidItems.Cast<DateTime>().Single());

        c = new ValidatingDateTimeCollection();
        c.Add("2024-07-31T09:07:03.250");
        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual(new DateTime(2024, 7, 31, 9, 7, 3, 250), c.ValidItems.Cast<DateTime>().Single());

        // Reject: space instead of 'T'
        c = new ValidatingDateTimeCollection();
        c.Add("2024-07-31 09:07:03");
        Assert.IsFalse(c.IsSatisfied);

        // Reject: single-digit hour
        c = new ValidatingDateTimeCollection();
        c.Add("2024-07-31T9:07");
        Assert.IsFalse(c.IsSatisfied);
    }

    [TestMethod]
    public void Bounds_DateTimeLocal_StrictFormats_OnBounds()
    {
        var f = new BoundsValidatingDateTimeCollectionFactory(
            new[] { "2024-01-01T00:00" },
            new[] { "2024-12-31T23:59:59.9999999" });
        var c = f.Create();

        c.Add("2024-06-01T12:00");
        Assert.IsTrue(c.IsSatisfied);

        c = f.Create();
        c.Add("2024-01-01T00:00");
        Assert.IsTrue(c.IsSatisfied);

        c = f.Create();
        c.Add("2024-12-31T23:59:59.9999999");
        Assert.IsTrue(c.IsSatisfied);

        c = f.Create();
        c.Add("2025-01-01T00:00");
        Assert.IsFalse(c.IsSatisfied);

        // Bad bounds should throw
        Assert.ThrowsException<FormatException>(() =>
            new BoundsValidatingDateTimeCollectionFactory(new[] { "2024-01-01 00:00" }, new[] { "2024-12-31T23:59" }));
    }
}