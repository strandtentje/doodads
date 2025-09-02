namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Factories;
public class CountingValidatingCollectionFactory(int lowerValueCountLimit, int upperValueCountLimit) : IValidatingCollectionFactory
{
    public bool CanConstrain => true;
    public IValidatingCollection Create() => new CountingValidatingCollection(lowerValueCountLimit, upperValueCountLimit);
}