using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace Ziewaar.RAD.Doodads.FormsValidation.HTML;
public class ValidatingWeekPicker(HtmlNode node) : IValidatingInputFieldInSet
{
    public event EventHandler<(string oldName, string newName)>? NameChanged;
    public string Name
    {
        get => node.GetInputName() ?? "";
        set
        {
            var oldName = node.GetInputName();
            node.SetInputName(value);
            foreach (var item in AltValidators)
            {
                item.Name = value;
            }
            if (oldName == null) return;
            NameChanged?.Invoke(this, (oldName, value));
        }
    }
    public DateOnly Max { get; private set; }
    public DateOnly Min { get; private set; }
    public int MinExpectedValues { get; set; }
    public int MaxExpectedValues { get; set; }    
    public List<IValidatingInputField> AltValidators { get; } = new();
    public bool IsRequired { get; private set; }
    public bool IsMaxUnbound => false;
    public bool TryValidate(string[] submittedValue, out IEnumerable result)
    {
        var readWeeks = submittedValue.Select(Parse).TakeWhile(x => x.wasParsed).Select(x => x.value)
            .Where(x => Min <= x && Max >= x).ToArray();
        result = readWeeks;
        return readWeeks.Length == submittedValue.Length;
    }
    public bool TryIdentityMerge(IValidatingInputFieldInSet otherFieldInSet)
    {
        if (otherFieldInSet.Name != this.Name)
            throw new FormValidationMarkupException("Cannot merge fields with different name");
        if (otherFieldInSet is ValidatingWeekPicker otherWeekPicker)
        {
            Min = otherWeekPicker.Min > Min ? otherWeekPicker.Min : Min;
            Max = otherWeekPicker.Max < Max ? otherWeekPicker.Max : Max;
            return true;
        }
        else
        {
            return false;
        }
    }
    public static (bool wasParsed, DateOnly value) ParseIsoWeek(string isoWeek)
    {
        // Example input: "2025-W28"
        if (!Regex.IsMatch(isoWeek, @"^\d{4}-W\d{2}$"))
            return (false,  DateOnly.MinValue);

        var parts = isoWeek.Split(new[] { "-W" }, StringSplitOptions.None);
        int year = int.Parse(parts[0]);
        int week = int.Parse(parts[1]);

        // ISO 8601: Week 1 is the one with the first Thursday of the year
        var jan4 = new DateTime(year, 1, 4); // always in week 1
        var startOfWeek1 = jan4.AddDays(-(int)jan4.DayOfWeek + 1); // Monday of week 1

        var weekStart = startOfWeek1.AddDays((week - 1) * 7);
        return (true, DateOnly.FromDateTime(weekStart));
    }
    
    private static (bool wasParsed, DateOnly value) Parse(string weekText) =>
        (DateOnly.TryParseExact(weekText + "-01", "yyyy-MM-dd", out var weekDate), weekDate);
    public static bool TryInsertInto(HtmlNode node, IValidatingInputFieldSet set)
    {
        if (node.GetInputTypeName() != "Week")
            return false;
        if (node.GetInputName() is not string inputName)
            return true;
        var isRequired = node.IsRequired();
        var maybeMin = Parse(node.GetMin() ?? $"{DateOnly.MinValue.Year}-W32");
        var maybeMax = Parse(node.GetMax() ?? $"{DateOnly.MaxValue.Year-1}-W32");
        set.Merge(new ValidatingWeekPicker(node)
        {
            IsRequired = isRequired,
            Min = maybeMin.wasParsed ? maybeMin.value : DateOnly.MinValue,
            Max = maybeMax.wasParsed ? maybeMax.value : DateOnly.MaxValue,
        });
        return true;
    }
}