namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Factories;

public class MultipartParameterValidationCollectionFactory(string expectedName) : IValidatingCollectionFactory
{
    public bool CanConstrain => true;
    public IValidatingCollection Create() => new MultipartParameterValidatingCollection(expectedName);
}