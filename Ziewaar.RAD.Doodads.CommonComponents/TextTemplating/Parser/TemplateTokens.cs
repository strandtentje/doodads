namespace Ziewaar.RAD.Doodads.CommonComponents.TextTemplating.Parser;

public static class TemplateTokens
{
    public static int OrdinalIndexOfPlaceholderStart(this string inputString, int fromPosition) =>
        inputString.IndexOf(PLACEHOLDER_START, fromPosition, StringComparison.Ordinal);

    public static int OrdinalIndexOfPlaceholderEnd(this string inputString, int fromPosition) =>
        inputString.IndexOf(PLACEHOLDER_END, fromPosition, StringComparison.Ordinal);

    public const string PLACEHOLDER_START = "{% ";
    public const string PLACEHOLDER_END = " %}";

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