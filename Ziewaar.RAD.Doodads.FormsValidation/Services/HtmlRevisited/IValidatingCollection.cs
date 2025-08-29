namespace Ziewaar.RAD.Doodads.FormsValidation.Services.HtmlRevisited;
public interface IValidatingCollection
{
    void Add(object value);
    bool IsSatisfied { get; }
    IEnumerable ValidItems { get; }
}