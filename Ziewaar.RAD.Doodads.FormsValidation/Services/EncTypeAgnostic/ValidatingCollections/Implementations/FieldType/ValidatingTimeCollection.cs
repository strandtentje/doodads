namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Implementations.FieldType;
public class ValidatingTimeCollection : IValidatingCollection
{
    private readonly List<object> BackingValues = new();
    private static readonly string[] Formats = new[]
    {
        "HH':'mm",
        "HH':'mm':'ss",
        "HH':'mm':'ss'.'FFFFFFF",
    };
    public void Add(object value, out object transformed)
    {
        transformed = value;
        if (!IsSatisfied) return;

        TimeOnly parsedTime;
        if (value is TimeOnly t)
        {
            parsedTime = t;
        }
        else
        {
            var s = value?.ToString();
            if (s is null ||
                !TimeOnly.TryParseExact(s, Formats, System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out parsedTime))
            {
                IsSatisfied = false;
                Reason = "Bad format";
                return;
            }
            else
            {
                transformed = parsedTime;
            }
        }

        if (IsSatisfied) BackingValues.Add(parsedTime);
    }

    public bool IsSatisfied { get; private set; } = true;
    public string Reason { get; private set; } = "";
    public IEnumerable ValidItems => BackingValues;
}