namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Factories.Operators;
public class AnyValidCollectionFactory(params IValidatingCollectionFactory?[] validators) : IValidatingCollectionFactory
{
    public bool CanConstrain => validators?.Length > 0;
    public IValidatingCollection? Create()
    {
        var actualValidators = validators.OfType<IValidatingCollectionFactory>().Select(x => x.Create())
            .OfType<IValidatingCollection>().ToArray();
        if (actualValidators.Length == 0)
            return null;
        return new AnyValidatingCollection(actualValidators);
    }
}