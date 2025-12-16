using Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Factories.Bounding;

namespace Ziewaar.RAD.Doodads.FormsValidation.Tests.Factories;
[TestClass]
public class BoundsFactoriesParsingFuzzTests
{
    // Verify that the factories: pick Max(lower bounds) / Min(upper bounds), handle duplicates, order, and throw on bad format.

    [TestMethod]
    public void NumberBounds_SelectsTightestWindow_AndParsesStrictly()
    {
        // lower bounds: -10, -3, -5 -> pick Max => -3
        // upper bounds: 10, 4, 7    -> pick Min => 4
        var f = new BoundsValidatingNumberCollectionFactory(
            new[] { "-10", "-3", "-5" },
            new[] { "10", "4", "7" }
        );
        var c = f.Create();

        c.Add("-3"); // inside
        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual(-3m, c.ValidItems.Cast<decimal>().Single());

        c = f.Create();
        c.Add("4");  // inside upper bound
        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual(4m, c.ValidItems.Cast<decimal>().Single());

        c = f.Create();
        c.Add("-3.0001"); // just below lower
        Assert.IsFalse(c.IsSatisfied);

        c = f.Create();
        c.Add("4.0001");  // just above upper
        Assert.IsFalse(c.IsSatisfied);
    }

    [TestMethod]
    public void NumberBounds_BadFormats_Throw()
    {
        // decimal.Parse without culture specified may accept commas based on current culture;
        // but "abc" should definitely throw.
        
        Assert.ThrowsExactly<FormatException>(() =>
            new BoundsValidatingNumberCollectionFactory(new[] { "abc" }, new[] { "5" }));

        Assert.ThrowsExactly<FormatException>(() =>
            new BoundsValidatingNumberCollectionFactory(new[] { "-1" }, new[] { "5x" }));
    }

    [TestMethod]
    public void DateBounds_TightestWindow_AndRejectsBad()
    {
        var f = new BoundsValidatingDateOnlyCollectionFactory(
            new[] { "2020-01-01", "2020-03-01" },    // pick 2020-03-01
            new[] { "2020-12-31", "2020-10-31" }     // pick 2020-10-31
        );
        var c = f.Create();

        c.Add("2020-06-15");
        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual(new DateOnly(2020,6,15), c.ValidItems.Cast<DateOnly>().Single());

        c = f.Create();
        c.Add("2020-02-29"); // below lower bound
        Assert.IsFalse(c.IsSatisfied);

        c = f.Create();
        c.Add("2020-11-01"); // above upper
        Assert.IsFalse(c.IsSatisfied);

        Assert.ThrowsExactly<FormatException>(() =>
            new BoundsValidatingDateOnlyCollectionFactory(new[] { "2020-02-30" }, new[] { "2020-12-31" }));
    }

    [TestMethod]
    public void DateTimeBounds_TightestWindow()
    {
        var f = new BoundsValidatingDateTimeCollectionFactory(
            new[] { "2020-01-01T00:00:00", "2020-06-01T00:00:00" },
            new[] { "2020-12-31T23:59:59", "2020-09-30T00:00:00" }
        );
        var c = f.Create();

        c.Add("2020-06-01T00:00:00");
        Assert.IsTrue(c.IsSatisfied);

        c = f.Create();
        c.Add("2020-09-30T00:00:00"); // equal to upper min is okay (inclusive)
        Assert.IsTrue(c.IsSatisfied);

        c = f.Create();
        c.Add("2020-05-31T23:59:59");
        Assert.IsFalse(c.IsSatisfied);
    }

    [TestMethod]
    public void TimeBounds_TightestWindow_AndRejectsBadFormat()
    {
        var f = new BoundsValidatingTimeCollectionFactory(
            new[] { "08:00", "09:00" }, // pick 09:00
            new[] { "18:00", "17:30" }  // pick 17:30
        );
        var c = f.Create();

        c.Add("09:00");
        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual(new TimeOnly(9,0), c.ValidItems.Cast<TimeOnly>().Single());

        c = f.Create();
        c.Add("17:30");
        Assert.IsTrue(c.IsSatisfied);

        c = f.Create();
        c.Add("08:59");
        Assert.IsFalse(c.IsSatisfied);

        Assert.ThrowsExactly<FormatException>(() =>
            new BoundsValidatingTimeCollectionFactory(new[] { "9-00" }, new[] { "17:30" }));
    }

    [TestMethod]
    public void WeekBounds_TightestWindow_AndRejectsBadFormat()
    {
        // week lower bounds => choose later week as lower; uppers => choose earlier week as upper
        var f = new BoundsValidatingWeekCollectionFactory(
            new[] { "2020-W01", "2020-W10" }, // pick W10 (later)
            new[] { "2020-W53", "2020-W20" }  // pick W20 (earlier)
        );
        var c = f.Create();

        c.Add("2020-W15"); // inside
        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual(new DateOnly(2020,4,6), c.ValidItems.Cast<DateOnly>().Single()); // Monday of W15

        c = f.Create();
        c.Add("2020-W09"); // below lower
        Assert.IsFalse(c.IsSatisfied);

        c = f.Create();
        c.Add("2020-W21"); // above upper
        Assert.IsFalse(c.IsSatisfied);

        Assert.ThrowsExactly<FormatException>(() =>
            new BoundsValidatingWeekCollectionFactory(new[] { "2020-15" }, new[] { "2020-W20" }));
    }

    [TestMethod]
    public void MonthBounds_TightestWindow_AndDemandsFullDateBounds()
    {
        // Bounds factory uses DateOnly.Parse; ensure it accepts full dates and we normalize added value
        var f = new BoundsValidatingMonthCollectionFactory(
            new[] { "2020-03-01", "2020-01-01" }, // pick 2020-03-01
            new[] { "2020-12-31", "2020-06-30" }  // pick 2020-06-30
        );
        var c = f.Create();

        c.Add("2020-04"); // normalized to 2020-04-01 -> inside
        Assert.IsTrue(c.IsSatisfied);
        Assert.AreEqual(new DateOnly(2020,4,1), c.ValidItems.Cast<DateOnly>().Single());

        c = f.Create();
        c.Add("2020-02");
        Assert.IsFalse(c.IsSatisfied);

        c = f.Create();
        c.Add("2020-07");
        Assert.IsFalse(c.IsSatisfied);
    }

    [TestMethod]
    public void BoundsFactories_EmptyArrays_DefaultMinMax_DoNotThrow_StillValidate()
    {
        // DateOnly: empty => Min/Max used
        var df = new BoundsValidatingDateOnlyCollectionFactory(Array.Empty<string>(), Array.Empty<string>());
        var dc = df.Create();
        dc.Add("0001-01-01");
        Assert.IsTrue(dc.IsSatisfied);

        // TimeOnly: empty => Min/Max used
        var tf = new BoundsValidatingTimeCollectionFactory(Array.Empty<string>(), Array.Empty<string>());
        var tc = tf.Create();
        tc.Add("00:00");
        Assert.IsTrue(tc.IsSatisfied);

        // Number: empty => decimal.Min/Max
        var nf = new BoundsValidatingNumberCollectionFactory(Array.Empty<string>(), Array.Empty<string>());
        var nc = nf.Create();
        nc.Add("0");
        Assert.IsTrue(nc.IsSatisfied);
    }

    [TestMethod]
    public void BoundsFactories_WhitespaceOrEmptyStrings_InBounds_ShouldThrow()
    {
        // Even if arrays are non-null, whitespace elements should fail parsing
        Assert.ThrowsExactly<FormatException>(() =>
            new BoundsValidatingDateTimeCollectionFactory(new[] { "   " }, new[] { "2020-01-01T00:00:00" }));

        Assert.ThrowsExactly<FormatException>(() =>
            new BoundsValidatingNumberCollectionFactory(new[] { "" }, new[] { "1" }));
    }
}