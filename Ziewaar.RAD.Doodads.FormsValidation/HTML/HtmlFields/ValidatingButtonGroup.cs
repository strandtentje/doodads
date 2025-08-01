using HtmlAgilityPack;

namespace Ziewaar.RAD.Doodads.FormsValidation.HTML;
public class ValidatingButtonGroup(HtmlNode node) : IValidatingInputFieldInSet
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

    public bool IsRequired => false; // buttons aren't typically required
    public bool IsMaxUnbound => false;

    public int MinExpectedValues { get; set; } = 0;
    public int MaxExpectedValues { get; set; } = 1;

    private HashSet<string> ValidValues { get; set; } = new();

    public List<IValidatingInputField> AltValidators { get; } = new();

    public static bool TryInsertInto(HtmlNode node, IValidatingInputFieldSet set)
    {
        if (node.Name != "button")
            return false;

        if (node.GetInputName() is not string inputName)
            return true;

        var value = node.GetAttributeValue("value", "").Trim();

        // Don't register button if it has no value (cannot validate)
        if (string.IsNullOrEmpty(value))
            return true;

        var buttonField = new ValidatingButtonGroup(node);
        buttonField.ValidValues.Add(value);

        set.Merge(buttonField);
        return true;
    }

    public bool TryValidate(string[] submittedValue, out IEnumerable result)
    {
        if (submittedValue.Length != 1)
        {
            result = Enumerable.Empty<object>();
            return false;
        }

        var value = submittedValue[0];
        if (ValidValues.Contains(value))
        {
            result = new[] { value };
            return true;
        }

        result = Enumerable.Empty<object>();
        return false;
    }

    public bool TryIdentityMerge(IValidatingInputFieldInSet otherFieldInSet)
    {
        if (otherFieldInSet.Name != this.Name)
            throw new FormValidationMarkupException("Cannot merge fields with different name");

        if (otherFieldInSet is ValidatingButtonGroup otherButtonGroup)
        {
            ValidValues.UnionWith(otherButtonGroup.ValidValues);
            return true;
        }

        return false;
    }
}