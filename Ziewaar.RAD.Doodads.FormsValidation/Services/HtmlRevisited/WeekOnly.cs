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
            int.TryParse(year, out var yearValue) && int.TryParse(week, out var weekValue) & weekValue > 0)
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
        // ISO-8601 week date:
        // - Week 1 is the week with Jan 4
        // - Weeks start on Monday (Mon=1..Sun=7)
        var jan4 = new DateTime(year, 1, 4);

        // Convert DayOfWeek (Sun=0..Sat=6) to ISO (Mon=1..Sun=7)
        int isoDow = ((int)jan4.DayOfWeek + 6) % 7 + 1;

        // Monday of ISO week 1
        var week1Monday = jan4.AddDays(1 - isoDow);

        // Start of requested week
        var weekStart = week1Monday.AddDays((week - 1) * 7);
        return DateOnly.FromDateTime(weekStart);
    }
}