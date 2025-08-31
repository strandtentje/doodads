namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Factories.FieldType;
public class ValidatingEmailCollectionFactory : IValidatingCollectionFactory
{
    public IValidatingCollection Create() => new ValidatingEmailCollection();
}