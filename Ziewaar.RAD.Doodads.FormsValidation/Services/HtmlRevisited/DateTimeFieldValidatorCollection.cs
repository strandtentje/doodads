namespace Ziewaar.RAD.Doodads.FormsValidation.Services.HtmlRevisited;
public class DateTimeFieldValidatorCollection : IValidatingCollection
{
    private readonly List<object> BackingValues = new();
    public void Add(object value)
    {
        IsSatisfied &= value is DateTime dateTime || DateTime.TryParse(value.ToString(), out dateTime);
        if (IsSatisfied)
            BackingValues.Add(dateTime);
    }
    public bool IsSatisfied { get; private set; } = true;
    public IEnumerable ValidItems => BackingValues;
}