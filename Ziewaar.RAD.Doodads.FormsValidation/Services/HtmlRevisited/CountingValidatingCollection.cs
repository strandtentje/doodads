namespace Ziewaar.RAD.Doodads.FormsValidation.Services.HtmlRevisited;
public class CountingValidatingCollection(int lowerValueCountLimit, int upperValueCountLimit) : IValidatingCollection
{    
    private readonly List<object> BackingValues = new();
    public bool IsSatisfied => BackingValues.Count >= lowerValueCountLimit && BackingValues.Count <= upperValueCountLimit;
    public IEnumerable ValidItems => BackingValues;
    public void Add(object value) => BackingValues.Add(value);
}