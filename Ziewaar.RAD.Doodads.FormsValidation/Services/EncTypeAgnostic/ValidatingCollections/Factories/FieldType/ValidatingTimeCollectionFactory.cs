namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Factories.FieldType;
public class ValidatingTimeCollectionFactory : IValidatingCollectionFactory
{
    public IValidatingCollection Create() => new ValidatingTimeCollection();
}