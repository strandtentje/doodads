using Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Factories;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Implementations.Operators;
public class AndValidatingCollection(IValidatingCollection[] precedents) : IValidatingCollection
{
    public void Add(object value, out object transformed)
    {
        foreach (var validatingCollection in precedents)
        {
            validatingCollection.Add(value, out var tempValue);
            if (tempValue is string tempString && string.IsNullOrEmpty((tempString)) || tempValue is Zero)
            {
                transformed = tempValue as string ?? "";
                return;
            }
            else
            {
                value = tempValue;
            }
        }
        transformed = value;
    }
    public bool IsSatisfied => precedents.All(x => x.IsSatisfied);
    public string Reason => string.Join(", ",  precedents.Where(x => !x.IsSatisfied).Select(x => x.Reason));
    public IEnumerable ValidItems => precedents.LastOrDefault()?.ValidItems ?? Enumerable.Empty<object>();
}