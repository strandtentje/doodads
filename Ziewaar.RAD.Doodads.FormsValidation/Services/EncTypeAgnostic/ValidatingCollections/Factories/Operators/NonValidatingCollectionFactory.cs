namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Factories.Operators;
public class NonValidatingCollectionFactory : IValidatingCollectionFactory
{
    public static readonly NonValidatingCollectionFactory Instance = new();
    public IValidatingCollection Create() => new NonValidatingCollection();
}