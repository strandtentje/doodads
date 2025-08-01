namespace Ziewaar.RAD.Doodads.CommonComponents.TextTemplating.Parser;
public static class TemplateTokens
{
    public enum ControlType
    {
        None,
        Placeholder,
        Locale
    }
    public static (ControlType type, int position) FindNextControlStart(this string inputString, int fromPosition)
    {
        var placeholderStart = inputString.IndexOf(PLACEHOLDER_START, fromPosition, StringComparison.Ordinal);
        var localeStart = inputString.IndexOf(LOCALE_START, fromPosition, StringComparison.Ordinal);
        if (placeholderStart >= fromPosition && (localeStart < fromPosition || placeholderStart < localeStart))
            return (ControlType.Placeholder, placeholderStart);
        else if (localeStart >= fromPosition && (placeholderStart < fromPosition || localeStart < placeholderStart))
            return (ControlType.Locale, localeStart);
        else
            return (ControlType.None, -1);
    }
    public static (ControlType type, int position) FindNextControlEnd(this string inputString, int fromPosition)
    {
        var placeholderEnd = inputString.IndexOf(PLACEHOLDER_END, fromPosition, StringComparison.Ordinal);
        var localeEnd = inputString.IndexOf(LOCALE_END, fromPosition, StringComparison.Ordinal);
        if (placeholderEnd >= fromPosition && (localeEnd < fromPosition || placeholderEnd < localeEnd))
            return (ControlType.Placeholder, placeholderEnd);
        else if (localeEnd >= fromPosition && (placeholderEnd < fromPosition || localeEnd < placeholderEnd))
            return (ControlType.Locale, localeEnd);
        else
            return (ControlType.None, -1);
    }
    public const string PLACEHOLDER_START = "{% ";
    public const string PLACEHOLDER_END = " %}";
    public const string LOCALE_START = "{@ ";
    public const string LOCALE_END = " @}";
    public static string TrimTemplateTokens(this string inputString) =>
        inputString.Trim(
            CALL_OUT_OR_CONTEXT, CONTEXT, CALL_OUT, ARGUMENT_SOURCE,
            NO_FILTER, HTML_FILTER, URL_FILTER, URL_DATA_FILTER, ATTRIBUTE_FILTER, JS_FILTER);
    public const char
        CALL_OUT_OR_CONTEXT = '?',
        CONTEXT = '<',
        CALL_OUT = '>',
        ARGUMENT_SOURCE = '#',
        NO_FILTER = '_',
        HTML_FILTER = '&',
        URL_FILTER = '%',
        URL_DATA_FILTER = '~',
        ATTRIBUTE_FILTER = '=',
        JS_FILTER = ';';
    public static TemplateCommandType ConvertToCommandType(this string tokens)
    {
        var sourceToken = TemplateCommandType.CallOutOrVariable;
        var filterToken = TemplateCommandType.NoFilter;
        foreach (var character in tokens)
        {
            switch (character)
            {
                case '<':
                    sourceToken = TemplateCommandType.VariableSource;
                    break;
                case '>':
                    sourceToken = TemplateCommandType.CallOutSource;
                    break;
                case '#':
                    sourceToken = TemplateCommandType.ConstantSource;
                    break;
                case '&':
                    filterToken = TemplateCommandType.HtmlFilter;
                    break;
                case '%':
                    filterToken = TemplateCommandType.UrlFilter;
                    break;
                case '~':
                    filterToken = TemplateCommandType.UrlDataFilter;
                    break;
                case '=':
                    filterToken = TemplateCommandType.AttributeFilter;
                    break;
                case ';':
                    filterToken = TemplateCommandType.JsFilter;
                    break;
                default:
                    break;
            }
        }
        return sourceToken | filterToken;
    }
}