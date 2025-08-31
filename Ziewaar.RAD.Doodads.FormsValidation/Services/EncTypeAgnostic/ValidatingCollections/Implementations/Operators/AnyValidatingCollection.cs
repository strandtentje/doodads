namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Implementations.Operators;
public class AnyValidatingCollection(IValidatingCollection[] collections) : IValidatingCollection
{
    public void Add(object value, out object transformed)
    {
        transformed = value;
        foreach (var validatingCollection in collections)
        {
            validatingCollection.Add(value, out var internalTransformed);
            if (validatingCollection.IsSatisfied)
                transformed = internalTransformed;
        }
    }
    public bool IsSatisfied => collections.Any(x => x.IsSatisfied);
    public IEnumerable ValidItems => collections.LastOrDefault(x => x.IsSatisfied)?.ValidItems ?? Enumerable.Empty<object>();
}

