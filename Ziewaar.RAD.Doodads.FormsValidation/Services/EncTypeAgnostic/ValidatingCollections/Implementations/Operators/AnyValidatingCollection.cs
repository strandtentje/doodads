namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Implementations.Operators;
public class AnyValidatingCollection(IValidatingCollection[] collections) : IValidatingCollection
{
    private readonly List<object> BackingStore = new();
    public void Add(object value, out object transformed)
    {
        transformed = value;
        foreach (var validatingCollection in collections)
        {
            validatingCollection.Add(value, out var internalTransformed);
            if (validatingCollection.IsSatisfied)
            {
                transformed = internalTransformed;
                BackingStore.Add(transformed);
                return;
            }
        }
    }
    public bool IsSatisfied => collections.Any(x => x.IsSatisfied);
    public string Reason => string.Join(", ",  collections.Where(x => !x.IsSatisfied).Select(x => x.Reason));
    public IEnumerable ValidItems => BackingStore;
}

