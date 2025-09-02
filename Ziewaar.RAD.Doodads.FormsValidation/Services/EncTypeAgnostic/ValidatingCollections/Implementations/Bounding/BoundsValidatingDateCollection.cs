namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Implementations.Bounding;
public class BoundsValidatingDateCollection(DateOnly lbound, DateOnly ubound) : IValidatingCollection
{
    private readonly List<object> BackingValues = new();
    private static readonly string[] DateFormats = new[]
    {
        "yyyy'-'MM'-'dd"
    };
    public void Add(object value, out object transformed)
    {
        transformed = value;
        if (!IsSatisfied) return;

        DateOnly parsed;

        if (value is DateOnly d)
        {
            parsed = d;
        }
        else if (value is DateTime dt)
        {
            parsed = DateOnly.FromDateTime(dt);
        }
        else
        {
            var s = value?.ToString();
            if (s is null ||
                !DateOnly.TryParseExact(s, DateFormats,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out parsed))
            {
                Reason = "Format incorrect";
                IsSatisfied = false;
                return;
            }
        }
        IsSatisfied &= parsed >= lbound && parsed <= ubound;
        if (IsSatisfied)
            BackingValues.Add(transformed = parsed);
        else
            Reason = "Out of bounds";
    }
    public bool IsSatisfied { get; private set; } = true;
    public string Reason { get; private set; } = "";
    public IEnumerable ValidItems => BackingValues;
}