#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.TextTemplating.Parser;
public struct TemplateCommand
{
    public int Position;
    public TemplateCommandType Type;
    public string PayloadText;
    public Func<object, string>? Formatter;
    public string Locale;
    public string GetFormattedPayload(object? overrideText)
    {
        if (Formatter != null)
        {
            return Formatter(overrideText ?? PayloadText);
        }
        else if (overrideText != null)
        {
            return Convert.ToString(overrideText);
        }
        else
        {
            return PayloadText;
        }
    }
}