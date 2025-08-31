namespace Ziewaar.RAD.Doodads.FormsValidation.Services.HtmlRevisited;
public interface IValidatingCollection
{
    void Add(object value, out object transformed);
    bool IsSatisfied { get; }
    IEnumerable ValidItems { get; }
}