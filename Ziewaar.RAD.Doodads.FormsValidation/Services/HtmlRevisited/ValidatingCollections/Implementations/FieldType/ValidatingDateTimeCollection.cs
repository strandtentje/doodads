namespace Ziewaar.RAD.Doodads.FormsValidation.Services.HtmlRevisited;
public class ValidatingDateTimeCollection : IValidatingCollection
{
    private readonly List<object> BackingValues = new();
    private static readonly string[] Formats = new[]
    {
        "yyyy'-'MM'-'dd'T'HH':'mm",
        "yyyy'-'MM'-'dd'T'HH':'mm':'ss",
        "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'FFFFFFF",
    };
    public void Add(object value, out object transformed)
    {
        transformed = value;

        DateTime parsed;
        if (value is DateTime dt)
        {
            parsed = dt;
        }
        else
        {
            var s = value?.ToString();
            if (s is null ||
                !DateTime.TryParseExact(s, Formats, System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out parsed))
            {
                IsSatisfied = false;
                return;
            }
        }

        if (IsSatisfied)
            BackingValues.Add(transformed = parsed);
    }
    public bool IsSatisfied { get; private set; } = true;
    public IEnumerable ValidItems => BackingValues;
}