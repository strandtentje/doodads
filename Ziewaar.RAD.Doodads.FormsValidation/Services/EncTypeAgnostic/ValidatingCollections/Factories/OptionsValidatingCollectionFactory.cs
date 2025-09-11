namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Factories;
public class OptionsValidatingCollectionFactory(string[] validOptions) : IValidatingCollectionFactory
{
    public bool CanConstrain => validOptions.Any();
    public IValidatingCollection Create() => new OptionsValidatingCollection(validOptions);
}