namespace Ziewaar.RAD.Doodads.FormsValidation.Services.HtmlRevisited;

public class ValidatingMonthCollection : IValidatingCollection
{
    private readonly System.Collections.Generic.List<object> BackingValues = new();
    public bool IsSatisfied { get; private set; } = true;
    public System.Collections.IEnumerable ValidItems => BackingValues;

    public void Add(object value, out object transformed)
    {
        transformed = value;
        if (!IsSatisfied) return;

        if (TryGetMonthStart(value, out var monthStart))
        {
            BackingValues.Add(transformed = monthStart);
        }
        else
        {
            IsSatisfied = false;
        }
    }

    private static bool TryGetMonthStart(object value, out DateOnly monthStart)
    {
        // Direct types
        if (value is DateOnly d)
        {
            monthStart = new DateOnly(d.Year, d.Month, 1);
            return true;
        }
        if (value is DateTime dt)
        {
            monthStart = new DateOnly(dt.Year, dt.Month, 1);
            return true;
        }

        // Strings and “stringy” objects
        var s = value?.ToString()?.Trim();
        if (string.IsNullOrEmpty(s))
        {
            monthStart = default;
            return false;
        }

        // 1) Year-Month exact formats (culture-invariant)
        var ymFormats = new[] { "yyyy-M", "yyyy-MM", "yyyy/M", "yyyy/MM" };
        if (DateTime.TryParseExact(s, ymFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var ym))
        {
            monthStart = new DateOnly(ym.Year, ym.Month, 1);
            return true;
        }

        // 2) Full date exact formats (culture-invariant)
        var ymdFormats = new[] { "yyyy-M-d", "yyyy-MM-dd", "yyyy/M/d", "yyyy/MM/dd" };
        if (DateTime.TryParseExact(s, ymdFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var ymd))
        {
            monthStart = new DateOnly(ymd.Year, ymd.Month, 1);
            return true;
        }

        // 3) Fallback: invariant general parse (handles ISO-like inputs)
        if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var generic))
        {
            monthStart = new DateOnly(generic.Year, generic.Month, 1);
            return true;
        }

        monthStart = default;
        return false;
    }
}