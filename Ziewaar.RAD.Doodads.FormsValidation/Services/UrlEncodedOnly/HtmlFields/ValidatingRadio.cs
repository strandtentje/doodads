namespace Ziewaar.RAD.Doodads.FormsValidation.Services.UrlEncodedOnly.HtmlFields;
public class ValidatingRadio(HtmlNode node) : IValidatingInputFieldInSet
{
    public event EventHandler<(string oldName, string newName)>? NameChanged;
    public string Name
    {
        get => node.GetInputName() ?? "";
        set
        {
            var oldName = node.GetInputName();
            node.SetInputName(value);
            if (oldName == null) return;
            NameChanged?.Invoke(this, (oldName, value));
        }
    }
    public bool IsRequired { get; private set; }
    public bool IsMaxUnbound => false;
    public int MinExpectedValues
    {
        get => IsRequired ? 1 : 0;
        set { }
    }
    public int MaxExpectedValues
    {
        get => 1;
        set { }
    }
    private IEnumerable<string> ValidValues { get; set; } = [];
    public List<IValidatingInputField> AltValidators { get; } = new();
    public static bool TryInsertInto(HtmlNode node, IValidatingInputFieldSet set)
    {
        if (node.GetInputTypeName() != "radio")
            return false;
        if (node.GetInputName() is not string)
            return true;
        if (node.GetInputValue() is string radioValue)
            set.Merge(new ValidatingRadio(node)
            {
                IsRequired = node.IsRequired(),                
                ValidValues = [radioValue]
            });
        return true;
    }
    public bool TryValidate(string[] submittedValue, out IEnumerable result)
    {
        if (submittedValue.Length == 1 && ValidValues.Contains(submittedValue.Single()) ||
            submittedValue.Length == 0 && !IsRequired)
        {
            result = submittedValue;
            return true;
        }
        else
        {
            result = Enumerable.Empty<object>();
            return false;
        }
    }
    public bool TryIdentityMerge(IValidatingInputFieldInSet otherFieldInSet)
    {
        if (otherFieldInSet.Name != this.Name)
            throw new FormValidationMarkupException("Cannot merge fields with different name");
        if (otherFieldInSet is ValidatingRadio otherRadio)
        {
            ValidValues = ValidValues.Concat(otherRadio.ValidValues);
            return true;
        }
        return false;
    }
}