namespace Ziewaar.RAD.Doodads.FormsValidation.Services.UrlEncodedOnly.HtmlFields;
public class ValidatingNumberPicker(HtmlNode node) : IValidatingInputFieldInSet
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
    public decimal Min { get; private set; }
    public decimal Max { get; private set; }
    public int MinExpectedValues { get; set; }
    public int MaxExpectedValues { get; set; }
    public List<IValidatingInputField> AltValidators { get; } = new();
    public bool IsRequired { get; private set; }
    public bool IsMaxUnbound => false;
    public bool TryValidate(string[] submittedValue, out IEnumerable result)
    {
        var readNumbers = submittedValue.Select(Parse).TakeWhile(x => x.wasParsed).Select(x => x.value)
            .Where(x => Min <= x && Max >= x).ToArray();
        result = readNumbers;
        return readNumbers.Length == submittedValue.Length;
    }
    public bool TryIdentityMerge(IValidatingInputFieldInSet otherFieldInSet)
    {
        if (otherFieldInSet.Name != this.Name)
            throw new FormValidationMarkupException("Cannot merge fields with different name");
        if (otherFieldInSet is ValidatingNumberPicker otherNumPicker)
        {
            Min = otherNumPicker.Min > Min ? otherNumPicker.Min : Min;
            Max = otherNumPicker.Max < Max ? otherNumPicker.Max : Max;
            return true;
        }
        else
        {
            return false;
        }
    }
    private static (bool wasParsed, decimal value) Parse(string numberText) =>
        (decimal.TryParse(numberText, CultureInfo.InvariantCulture, out var monthDate), monthDate);
    public static bool TryInsertInto(HtmlNode node, IValidatingInputFieldSet set)
    {
        if (node.GetInputTypeName() != "number" && node.GetInputTypeName() != "range")
            return false;
        if (node.GetInputName() is not string inputName)
            return true;
        var isRequired = node.IsRequired();
        var maybeMin = Parse(node.GetMin() ?? $"{decimal.MinValue}");
        var maybeMax = Parse(node.GetMax() ?? $"{decimal.MaxValue}");
        set.Merge(new ValidatingNumberPicker(node)
        {
            IsRequired = isRequired,
            Min = maybeMin.wasParsed ? maybeMin.value : decimal.MinValue,
            Max = maybeMax.wasParsed ? maybeMax.value : decimal.MaxValue,
        });
        return true;
    }
}