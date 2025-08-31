namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Factories.Operators;
public class AnyValidCollectionFactory(params IValidatingCollectionFactory[] validators) : IValidatingCollectionFactory
{
    public IValidatingCollection Create() => new AnyValidatingCollection(
        [..validators.Select(x => x.Create())]);
}