namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Factories;
public class OptionsValidatingCollectionFactory(string[] validOptions) : IValidatingCollectionFactory
{
    public IValidatingCollection Create() => new OptionsValidatingCollection(validOptions);
}