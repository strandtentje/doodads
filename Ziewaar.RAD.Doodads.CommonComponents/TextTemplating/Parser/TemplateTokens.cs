namespace Ziewaar.RAD.Doodads.CommonComponents;

public static class TemplateTokens
{
    public static int OrdinalIndexOfPlaceholderStart(this string inputString, int fromPosition) =>
        inputString.IndexOf(PlaceholderStart, fromPosition, StringComparison.Ordinal);

    public static int OrdinalIndexOfPlaceholderEnd(this string inputString, int fromPosition) =>
        inputString.IndexOf(PlaceholderEnd, fromPosition, StringComparison.Ordinal);

    public const string PlaceholderStart = "{% ";
    public const string PlaceholderEnd = " %}";

    public static string TrimTemplateTokens(this string inputString) =>
        inputString.Trim(
            CallOutOrContext, Context, CallOut, ContextCallOut, ArgumentSource,
            NoFilter, HtmlFilter, UrlFilter, AttributeFilter, JsFilter);

    public const char
        CallOutOrContext = '?',
        Context = '<',
        CallOut = '>',
        ContextCallOut = ':',
        ArgumentSource = '#',
        NoFilter = '_',
        HtmlFilter = '&',
        UrlFilter = '%',
        AttributeFilter = '=',
        JsFilter = ';';

    public static TemplateCommandType ConvertToSourceCommandType(this char token) => token switch
    {
        '<' => TemplateCommandType.VariableSource,
        '>' => TemplateCommandType.CallOutSource,
        ':' => TemplateCommandType.ContextCallOutSource,
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