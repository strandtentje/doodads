using Ziewaar.RAD.Doodads.FormsValidation.Services.Support;

namespace Ziewaar.RAD.Doodads.FormsValidation.Tests;
[TestClass]
public class WeekOnlyIsoTests
{
    [TestMethod]
    public void ToDateOnly_Week1_SundayJan4_UsesPrevYearMonday()
    {
        // 2015-01-04 was a Sunday; ISO week 1 starts Monday 2014-12-29
        var w = new WeekOnly(2015, 1);
        Assert.AreEqual(new DateOnly(2014, 12, 29), w.ToDateOnly());
    }

    [TestMethod]
    public void ToDateOnly_Week1_MondayJan4_UsesThatMonday()
    {
        // 2021-01-04 was a Monday; ISO week 1 starts 2021-01-04
        var w = new WeekOnly(2021, 1);
        Assert.AreEqual(new DateOnly(2021, 1, 4), w.ToDateOnly());
    }

    [TestMethod]
    public void ToDateOnly_Week53_2020_Is2020_12_28()
    {
        var w = new WeekOnly(2020, 53);
        Assert.AreEqual(new DateOnly(2020, 12, 28), w.ToDateOnly());
    }
}