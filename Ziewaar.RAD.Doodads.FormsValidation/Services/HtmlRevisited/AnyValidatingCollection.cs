namespace Ziewaar.RAD.Doodads.FormsValidation.Services.HtmlRevisited;
public class AnyValidatingCollection(IValidatingCollection[] collections) : IValidatingCollection
{
    public void Add(object value)
    {
        foreach (var validatingCollection in collections)
            validatingCollection.Add(value);
    }
    public bool IsSatisfied => collections.Any(x => x.IsSatisfied);
    public IEnumerable ValidItems => collections.Last(x => x.IsSatisfied).ValidItems;
}