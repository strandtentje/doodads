using System.Collections.Concurrent;
using Ziewaar.RAD.Doodads.CommonComponents.TextActions;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Factories;

public class PatternValidationCollectionFactory(string pattern) : IValidatingCollectionFactory
{
    public bool CanConstrain => !string.IsNullOrEmpty(pattern);
    private readonly Regex Pattern = new($"^{pattern}$");
    public IValidatingCollection Create() => new PatternValidatingCollection(Pattern);
}