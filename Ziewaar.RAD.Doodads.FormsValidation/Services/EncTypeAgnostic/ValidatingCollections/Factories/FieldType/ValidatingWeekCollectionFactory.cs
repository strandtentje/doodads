namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Factories.FieldType;
public class ValidatingWeekCollectionFactory : IValidatingCollectionFactory
{
    public IValidatingCollection Create() => new ValidatingWeekCollection();
}