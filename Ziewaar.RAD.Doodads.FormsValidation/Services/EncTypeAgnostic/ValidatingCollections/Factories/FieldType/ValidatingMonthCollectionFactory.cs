namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Factories.FieldType;
public class ValidatingMonthCollectionFactory : IValidatingCollectionFactory
{
    public IValidatingCollection Create() => new ValidatingMonthCollection();
}