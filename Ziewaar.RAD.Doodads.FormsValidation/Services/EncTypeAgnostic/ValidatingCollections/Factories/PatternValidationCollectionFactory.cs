namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Factories;
public class PatternValidationCollectionFactory(string pattern) : IValidatingCollectionFactory
{
    private readonly Regex Pattern = new($"^{pattern}$");
    public IValidatingCollection Create() => new PatternValidatingCollection(Pattern);
}