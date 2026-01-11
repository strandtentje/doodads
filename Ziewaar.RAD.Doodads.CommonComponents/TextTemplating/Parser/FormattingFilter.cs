namespace Ziewaar.RAD.Doodads.CommonComponents.TextTemplating.Parser;

public class FormattingFilter : ITemplateFilter
{
    private FormattingFilter(string formatString, string cleanPayload, TemplateCommandType command) =>
        (FormatString, CleanPayload, Command) = (formatString, cleanPayload, command);

    public static bool TryParse(string dirtyPayload, out ITemplateFilter? result)
    {
        var colonPayload = dirtyPayload.Trim().TrimTemplateTokens();
        var splitPayload = colonPayload.Split([':'], count: 2);
        var cleanPayload = splitPayload.ElementAtOrDefault(0) ?? "";
        var formatter = splitPayload.ElementAtOrDefault(1) ?? "";
        var modifiers = dirtyPayload.Substring(0, dirtyPayload.Length - colonPayload.Length);

        result = null;
        if (!string.IsNullOrWhiteSpace(formatter))
            result = new FormattingFilter(formatter, cleanPayload, modifiers.ConvertToCommandType());
        return result != null;
    }

    private string FormatString;
    public string CleanPayload { get; }
    public TemplateCommandType Command { get; }

    public string Render(object value)
    {
        if (value.ConvertNumericToDecimal() is decimal decimalValue)
            return decimalValue.ToString(FormatString);
        else if (value is DateTime dateTimeValue)
            return dateTimeValue.ToString(FormatString);
        else if (value is TimeSpan timespanValue)
            return timespanValue.ToString(FormatString);
        else
            return value.ToString();
    }
}