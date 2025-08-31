using System.Text.RegularExpressions;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.HtmlRevisited;
public class PatternValidatingCollection(Regex pattern) : IValidatingCollection
{
    private readonly List<object> BackingValues = new();
    public bool IsSatisfied { get; private set; } = true;
    public IEnumerable ValidItems => BackingValues;
    public void Add(object value, out object transformed)
    {
        transformed = value;
        if (!IsSatisfied) return;
        IsSatisfied &= pattern.IsMatch(value.ToString() ?? "");
        if (IsSatisfied)
            BackingValues.Add(transformed = value.ToString() ?? "");
    }
}