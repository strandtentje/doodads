namespace Ziewaar.RAD.Doodads.FormsValidation.Services.UrlEncodedOnly.HtmlFields;
public class ValidatingDateTimeLocalPicker(HtmlNode node) : IValidatingInputFieldInSet
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
    public DateTime Min { get; private set; }
    public DateTime Max { get; private set; }
    public bool IsMaxUnbound => false;
    public int MinExpectedValues { get; set; }
    public int MaxExpectedValues { get; set; }
    public bool IsRequired { get; private set; }
    public List<IValidatingInputField> AltValidators { get; } = new();
    public static bool TryInsertInto(HtmlNode node, IValidatingInputFieldSet set)
    {
        if (node.GetInputTypeName() != "datetime-local")
            return false;
        if (node.GetInputName() is not string inputName)
            return true;

        var minValue = DateTime.TryParse(node.GetMin() ?? DateTime.MinValue.ToString(), out var candidateMinValue)
            ? candidateMinValue
            : DateTime.MinValue;
        var maxValue = DateTime.TryParse(node.GetMax() ?? DateTime.MaxValue.ToString(), out var candidateMaxValue)
            ? candidateMaxValue
            : DateTime.MaxValue;
        var isRequired = node.IsRequired();
        set.Merge(new ValidatingDateTimeLocalPicker(node)
        {
            IsRequired = isRequired, Min = minValue, Max = maxValue
        });
        return true;
    }
    private (bool wasParsed, DateTime value) Parse(string dateText) =>
        (DateTime.TryParse(dateText, out var dt), dt);
    public bool TryValidate(string[] submittedValue, out IEnumerable result)
    {
        var readDates = submittedValue.Select(Parse).TakeWhile(x => x.wasParsed).Select(x => x.value)
            .Where(x => Min <= x && Max >= x).ToArray();
        result = readDates;
        return readDates.Length == submittedValue.Length;
    }
    public bool TryIdentityMerge(IValidatingInputFieldInSet otherFieldInSet)
    {
        if (otherFieldInSet.Name != this.Name)
            throw new FormValidationMarkupException("Cannot merge fields with different name");
        if (otherFieldInSet is ValidatingDateTimeLocalPicker otherDatePicker)
        {
            Min = otherDatePicker.Min > Min ? otherDatePicker.Min : Min;
            Max = otherDatePicker.Max < Max ? otherDatePicker.Max : Max;
            return true;
        }
        else
        {
            return false;
        }
    }
}