using HtmlAgilityPack;

namespace Ziewaar.RAD.Doodads.FormsValidation.HTML;
public class ValidatingMonthPicker : IValidatingInputFieldInSet
{
    public DateOnly Max { get; private set; }
    public DateOnly Min { get; private set; }
    public int MinExpectedValues { get; set; }
    public int MaxExpectedValues { get; set; }
    public string Name { get; private set; }
    public List<IValidatingInputField> AltValidators { get; } = new();
    public bool IsRequired { get; private set; }
    public bool IsMaxUnbound => false;
    public bool TryValidate(string[] submittedValue, out object? result)
    {
        var readMonths = submittedValue.Select(Parse).TakeWhile(x => x.wasParsed).Select(x => x.value)
            .Where(x => Min <= x && Max >= x).ToArray();
        result = readMonths;
        return readMonths.Length == submittedValue.Length;
    }
    public bool TryIdentityMerge(IValidatingInputFieldInSet otherFieldInSet)
    {
        if (otherFieldInSet.Name != this.Name)
            throw new FormValidationMarkupException("Cannot merge fields with different name");
        if (otherFieldInSet is ValidatingMonthPicker otherDatePicker)
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
    private static (bool wasParsed, DateOnly value) Parse(string monthText) =>
        (DateOnly.TryParseExact(monthText + "-01", "yyyy-MM-dd", out var monthDate), monthDate);
    public static bool TryInsertInto(HtmlNode node, IValidatingInputFieldSet set)
    {
        if (node.GetInputTypeName() != "month")
            return false;
        if (node.GetInputName() is not string inputName)
            return true;
        var isRequired = node.IsRequired();
        var maybeMin = Parse(node.GetMin() ?? $"{DateOnly.MinValue.Year}-{DateOnly.MinValue.Month}");
        var maybeMax = Parse(node.GetMax() ?? $"{DateOnly.MaxValue.Year}-{DateOnly.MaxValue.Month}");
        set.Merge(new ValidatingMonthPicker()
        {
            Name = inputName, IsRequired = isRequired,
            Min = maybeMin.wasParsed ? maybeMin.value : DateOnly.MinValue,
            Max = maybeMax.wasParsed ? maybeMax.value : DateOnly.MaxValue,
        });
        return true;
    }
}