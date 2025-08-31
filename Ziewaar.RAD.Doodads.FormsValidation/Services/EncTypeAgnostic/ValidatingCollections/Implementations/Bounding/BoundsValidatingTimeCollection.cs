namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Implementations.Bounding;
public class BoundsValidatingTimeCollection(TimeOnly lbound, TimeOnly ubound) : IValidatingCollection
{
    private readonly List<object> BackingValues = new();
    private static readonly string[] Formats = new[]
    {
        "HH':'mm",
        "HH':'mm':'ss",
        "HH':'mm':'ss'.'FFFFFFF", // up to 7 fractional seconds
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
                return;
            }
            else
            {
                transformed = parsedTime;
            }
        }
        IsSatisfied &= parsedTime >= lbound && parsedTime <= ubound;
        if (IsSatisfied) BackingValues.Add(transformed = parsedTime);
    }
    public bool IsSatisfied { get; private set; } = true;
    public IEnumerable ValidItems => BackingValues;
}