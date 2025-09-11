namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Implementations.Bounding;
public class BoundsValidatingNumberCollection(decimal lBound, decimal uBound) : IValidatingCollection
{
    private readonly List<object> BackingValues = new();
    public bool IsSatisfied { get; private set; } = true;
    public string Reason { get; private set; }
    public IEnumerable ValidItems => BackingValues;
    public void Add(object value, out object transformed)
    {
        transformed = value;
        if (!IsSatisfied)
            return;
        IsSatisfied &= value is decimal dy || decimal.TryParse($"{value}", CultureInfo.InvariantCulture, out dy);
        IsSatisfied &= dy >= lBound && dy <= uBound;
        if (IsSatisfied)
            BackingValues.Add(transformed = dy);
        else
            Reason = "Bad format or out of bounds";
    }
}