using System.Text.RegularExpressions;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.HtmlRevisited;
public class PatternValidationCollectionFactory(string pattern) : IValidatingCollectionFactory
{
    private readonly Regex Pattern = new($"^{pattern}$");
    public IValidatingCollection Create() => new PatternValidatingCollection(Pattern);
}