namespace Ziewaar.RAD.Doodads.CommonComponents;

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
            NO_FILTER, HTML_FILTER, URL_FILTER, ATTRIBUTE_FILTER, JS_FILTER);

    public const char
        CALL_OUT_OR_CONTEXT = '?',
        CONTEXT = '<',
        CALL_OUT = '>',
        ARGUMENT_SOURCE = '#',
        NO_FILTER = '_',
        HTML_FILTER = '&',
        URL_FILTER = '%',
        ATTRIBUTE_FILTER = '=',
        JS_FILTER = ';';

    public static TemplateCommandType ConvertToSourceCommandType(this char token) => token switch
    {
        '<' => TemplateCommandType.VariableSource,
        '>' => TemplateCommandType.CallOutSource,
        '#' => TemplateCommandType.ConstantSource,
        _ => TemplateCommandType.CallOutOrVariable,
    };

    public static TemplateCommandType ConvertToFilterCommandType(this char token) => token switch
    {
        '&' => TemplateCommandType.HtmlFilter,
        '%' => TemplateCommandType.UrlFilter,
        '=' => TemplateCommandType.AttributeFilter,
        ';' => TemplateCommandType.JsFilter,
        _ => TemplateCommandType.NoFilter,
    };
}