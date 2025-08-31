namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Factories.FieldType;
public class ValidatingNumberCollectionFactory : IValidatingCollectionFactory
{
    public IValidatingCollection Create() => new ValidatingNumberCollection();
}