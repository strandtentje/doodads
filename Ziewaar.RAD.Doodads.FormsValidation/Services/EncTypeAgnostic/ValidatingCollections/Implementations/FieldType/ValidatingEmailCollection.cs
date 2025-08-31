namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Implementations.FieldType;
public class ValidatingEmailCollection : IValidatingCollection
{
    private readonly List<object> BackingValues = new();
    public void Add(object value, out object transformed)
    {
        IsSatisfied &= EmailValidation.EmailValidator.Validate((transformed = value).ToString());
        if (IsSatisfied)
            BackingValues.Add(value);
    }
    public bool IsSatisfied { get; private set; } = true;
    public IEnumerable ValidItems => BackingValues;
}