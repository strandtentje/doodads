namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Factories.FieldType;
public class ValidatingFileCollectionFactory : IValidatingCollectionFactory
{
    public IValidatingCollection Create() => new ValidatingFileCollection();
}