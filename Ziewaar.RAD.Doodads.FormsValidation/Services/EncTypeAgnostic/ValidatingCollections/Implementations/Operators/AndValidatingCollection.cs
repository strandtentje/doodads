namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Implementations.Operators;
public class AndValidatingCollection(IValidatingCollection[] precedents) : IValidatingCollection
{
    public void Add(object value, out object transformed)
    {
        foreach (var validatingCollection in precedents)
            validatingCollection.Add(value, out value);
        transformed = value;
    }
    public bool IsSatisfied => precedents.All(x => x.IsSatisfied);
    public IEnumerable ValidItems => precedents.LastOrDefault()?.ValidItems ?? Enumerable.Empty<object>();
}