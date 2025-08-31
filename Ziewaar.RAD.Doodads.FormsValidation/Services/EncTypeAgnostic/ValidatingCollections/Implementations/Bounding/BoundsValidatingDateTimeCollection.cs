namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Implementations.Bounding;
public class BoundsValidatingDateTimeCollection(DateTime lBound, DateTime uBound) : IValidatingCollection
{
    public List<object> BackingValues = new();
    public bool IsSatisfied { get; private set; } = true;
    public IEnumerable ValidItems => BackingValues;    private static readonly string[] Formats = new[]
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
        else if (value is DateOnly dto)
        {
            parsed = dto.ToDateTime(TimeOnly.MinValue);
        } else
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
        IsSatisfied &= parsed >= lBound && parsed <= uBound;
        if (IsSatisfied)
            BackingValues.Add(transformed = parsed);
    }
}