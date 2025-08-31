namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Factories.FieldType;
public class ValidatingColorCollectionFactory : IValidatingCollectionFactory
{
    public IValidatingCollection Create() => new ValidatingColorCollection();
}