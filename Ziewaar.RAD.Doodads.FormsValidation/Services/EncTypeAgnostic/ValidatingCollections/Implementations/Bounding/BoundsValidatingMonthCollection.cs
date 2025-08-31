namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Implementations.Bounding;
public class BoundsValidatingMonthCollection(DateOnly lBound, DateOnly uBound) : IValidatingCollection
{
    private readonly List<object> BackingValues = new();
    public bool IsSatisfied { get; private set; } = true;
    public IEnumerable ValidItems => BackingValues;
    public void Add(object value, out object transformed)
    {
        transformed = value;
        if (!IsSatisfied)
            return;
        value = value is DateTime dtValue ? DateOnly.FromDateTime(dtValue) : value;
        IsSatisfied &= value is DateOnly dy || DateOnly.TryParse($"{value}-01", out dy);
        IsSatisfied &= dy >= lBound &&  dy <= uBound;
        if (IsSatisfied)
            BackingValues.Add(transformed =  dy);
    }
}