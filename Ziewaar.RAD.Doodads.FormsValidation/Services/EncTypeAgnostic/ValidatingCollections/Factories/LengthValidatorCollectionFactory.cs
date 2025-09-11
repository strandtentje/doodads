namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Factories;
public class LengthValidatorCollectionFactory(uint minLength, uint maxLength) : IValidatingCollectionFactory
{
    public bool CanConstrain => true;
    public IValidatingCollection Create() => new LengthValidatorCollection(minLength, maxLength);
}