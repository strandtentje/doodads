namespace Ziewaar.RAD.Doodads.FormsValidation.Services.HtmlRevisited;
public class EmailFieldValidatorCollection : IValidatingCollection
{
    private readonly List<object> BackingValues = new();
    public void Add(object value)
    {
        IsSatisfied &= EmailValidation.EmailValidator.Validate(value.ToString());
        if (IsSatisfied)
            BackingValues.Add(value);
    }
    public bool IsSatisfied { get; private set; } = true;
    public IEnumerable ValidItems => BackingValues;
}