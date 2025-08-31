namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Factories.FieldType;
public class ValidatingDateTimeCollectionFactory : IValidatingCollectionFactory
{
    public IValidatingCollection Create() => new ValidatingDateTimeCollection();
}