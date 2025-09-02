namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Factories.FieldType;
public class ValidatingDateOnlyCollectionFactory : IValidatingCollectionFactory
{
    public IValidatingCollection Create() => new ValidatingDateCollection();
    public bool CanConstrain => true;
}