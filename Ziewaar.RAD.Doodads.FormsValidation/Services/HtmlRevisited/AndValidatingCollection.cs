namespace Ziewaar.RAD.Doodads.FormsValidation.Services.HtmlRevisited;
public class AndValidatingCollection(IValidatingCollection[] precedents) : IValidatingCollection
{
    public void Add(object value)
    {
        foreach (var validatingCollection in precedents)
            validatingCollection.Add(value);
    }
    public bool IsSatisfied => precedents.All(x => x.IsSatisfied);
    public IEnumerable ValidItems => precedents.Last().ValidItems;
}