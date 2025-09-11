namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Factories.FieldType;
public class ValidatingMonthCollectionFactory : IValidatingCollectionFactory
{
    public bool CanConstrain => true;
    public IValidatingCollection Create() => new ValidatingMonthCollection();
}