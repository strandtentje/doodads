namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Interfaces;
public interface IValidatingCollectionFactory
{
    bool CanConstrain { get; }
    IValidatingCollection? Create();
}