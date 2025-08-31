namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Factories.Operators;
public class AllValidCollectionsFactory(params IValidatingCollectionFactory[] precedents) 
    : IValidatingCollectionFactory
{
    public IValidatingCollection Create() => new AndValidatingCollection(
        [..precedents.Select(x => x.Create())]);
}