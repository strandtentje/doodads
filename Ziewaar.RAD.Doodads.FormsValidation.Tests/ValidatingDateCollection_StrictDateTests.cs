namespace Ziewaar.RAD.Doodads.FormsValidation.Tests;
[TestClass]
public class ValidatingDateCollectionStrictDateTests
{
    [TestMethod]
    public void Accepts_DateOnly_And_StoresAsDateOnly()
    {
        var c = new ValidatingDateCollection();
        c.Add(new DateOnly(2024, 7, 31));
        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual(new DateOnly(2024, 7, 31), c.ValidItems.Cast<DateOnly>().Single());
    }

    [TestMethod]
    public void Accepts_DateTime_NormalizesToDateOnly()
    {
        var c = new ValidatingDateCollection();
        c.Add(new DateTime(2024, 7, 31, 15, 45, 00));
        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual(new DateOnly(2024, 7, 31), c.ValidItems.Cast<DateOnly>().Single());
    }

    [TestMethod]
    public void Accepts_StrictIsoDate_String()
    {
        var c = new ValidatingDateCollection();
        c.Add("2024-07-31");
        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual(new DateOnly(2024, 7, 31), c.ValidItems.Cast<DateOnly>().Single());
    }

    [TestMethod]
    public void Rejects_NonStrictFormats()
    {
        // Not zero-padded and non-ISO forms should fail under ParseExact.
        var bad = new[]
        {
            "2024-7-1",       // not zero-padded
            "31/07/2024",     // different culture style
            "2024/07/31",     // wrong separator
            "2024-07-31T00:00",
            " 2024-07-31 ",
            "",
            null
        };

        foreach (var s in bad)
        {
            var c = new ValidatingDateCollection();
            c.Add(s!);
            Assert.IsFalse(c.IsSatisfied, $"Should reject '{s ?? "<null>"}'");
        }
    }
}