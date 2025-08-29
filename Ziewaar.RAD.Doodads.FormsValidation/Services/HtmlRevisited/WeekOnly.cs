namespace Ziewaar.RAD.Doodads.FormsValidation.Services.HtmlRevisited;
public class WeekOnly(int year, int week)
{
    public static WeekOnly Parse(string value)
    {
        if (WeekOnly.TryParse(value, out var result))
            return result;
        throw new FormatException(
            "Invalid week format; must be ####-W## such that #### is the year and ## is the week number");
    }
    public static bool TryParse(string fieldValue, [NotNullWhen(true)] out WeekOnly? result)
    {
        result = null;
        if (fieldValue.Length != "1970-W1".Length && fieldValue.Length != "1970-W11".Length)
            return false;
        var splitPos = fieldValue.IndexOf("-W", StringComparison.Ordinal);
        if (splitPos != 4)
            return false;
        var year = fieldValue.Substring(0, 4);
        var week = fieldValue.Substring(6);
        if (year.All(char.IsAsciiDigit) && week.All(char.IsAsciiDigit) && week.Length is < 3 and > 0 &&
            int.TryParse(year, out var yearValue) && int.TryParse(week, out var weekValue))
        {
            result = new(year: yearValue, week: weekValue);
            return true;
        }
        else
        {
            return false;
        }
    }
    public DateOnly ToDateOnly()
    {
        var jan4 = new DateTime(year, 1, 4); // always in week 1
        var startOfWeek1 = jan4.AddDays(-(int)jan4.DayOfWeek + 1); // Monday of week 1
        var weekStart = startOfWeek1.AddDays((week - 1) * 7);
        return DateOnly.FromDateTime(weekStart);
    }
}