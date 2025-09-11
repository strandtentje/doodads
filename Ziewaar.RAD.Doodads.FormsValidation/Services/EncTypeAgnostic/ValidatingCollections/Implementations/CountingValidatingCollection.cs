namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Implementations;
public class CountingValidatingCollection(int lowerValueCountLimit, int upperValueCountLimit) : IValidatingCollection
{    
    private readonly List<object> BackingValues = new();
    public bool IsSatisfied => BackingValues.Count >= lowerValueCountLimit && BackingValues.Count <= upperValueCountLimit;
    public string Reason => IsSatisfied ? "" : "Too many values";
    public IEnumerable ValidItems => BackingValues;
    public void Add(object value, out object transformed) => BackingValues.Add(transformed = value);
}