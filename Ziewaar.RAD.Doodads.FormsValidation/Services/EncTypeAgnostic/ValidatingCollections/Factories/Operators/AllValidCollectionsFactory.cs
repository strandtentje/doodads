namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Factories.Operators;
public class AllValidCollectionsFactory(params IValidatingCollectionFactory?[] precedents) 
    : IValidatingCollectionFactory
{
    public bool CanConstrain => precedents?.Length > 0;
    public IValidatingCollection? Create()
    {
        var actualValidators = precedents.OfType<IValidatingCollectionFactory>().Select(x => x.Create()).OfType<IValidatingCollection>().ToArray();
        if (actualValidators.Length == 0) return null;
        return new AndValidatingCollection(actualValidators);
    }
}