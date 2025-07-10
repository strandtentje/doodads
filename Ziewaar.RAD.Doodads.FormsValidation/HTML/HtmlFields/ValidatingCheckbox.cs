using HtmlAgilityPack;

namespace Ziewaar.RAD.Doodads.FormsValidation.HTML;
public class ValidatingCheckbox(HtmlNode node) : IValidatingInputFieldInSet
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
    public int MinExpectedValues { get; set; }
    public int MaxExpectedValues { get; set; }
    private IEnumerable<string> ValidValues { get; set; } = [];
    public List<IValidatingInputField> AltValidators { get; } = new();
    public static bool TryInsertInto(HtmlNode node, IValidatingInputFieldSet set)
    {
        if (node.GetInputTypeName() != "checkbox")
            return false;
        if (node.GetInputName() is not string inputName)
            return true;
        if (node.GetInputValue() is string checkboxValue)
            set.Merge(new ValidatingCheckbox(node)
            {
                IsRequired = node.IsRequired(),
                ValidValues = [checkboxValue],
            });
        return true;
    }
    public bool TryValidate(string[] submittedValue, out IEnumerable result)
    {
        if (submittedValue.All(ValidValues.Contains))
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
        if (otherFieldInSet is ValidatingCheckbox otherCheckbox)
        {
            ValidValues = ValidValues.Concat(otherCheckbox.ValidValues);
            return true;
        }
        return false;
    }
}
