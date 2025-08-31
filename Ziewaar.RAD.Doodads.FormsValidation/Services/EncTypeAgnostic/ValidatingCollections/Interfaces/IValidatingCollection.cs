namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Interfaces;
public interface IValidatingCollection
{
    void Add(object value, out object transformed);
    bool IsSatisfied { get; }
    IEnumerable ValidItems { get; }
}