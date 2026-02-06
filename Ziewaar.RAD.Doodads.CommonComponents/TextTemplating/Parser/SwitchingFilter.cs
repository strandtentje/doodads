using System.Text.RegularExpressions;

namespace Ziewaar.RAD.Doodads.CommonComponents.TextTemplating.Parser;

public class SwitchingFilter : ITemplateFilter
{
    private readonly (System.Text.RegularExpressions.Regex Regex, ITemplateFilter Filter)[] Mappings;
    public string CleanPayload { get; }
    public TemplateCommandType Command { get; }

    private SwitchingFilter(string cleanPayload, TemplateCommandType templateCommandType,
        (Regex Regex, ITemplateFilter Filter)[] mappings) =>
        (CleanPayload, Command, Mappings) = (cleanPayload, templateCommandType, mappings);

    public static bool TryParse(string dirtyPayload, out ITemplateFilter? result)
    {
        var trimmedPayload = dirtyPayload.Trim();
        var preamble = trimmedPayload.TakeWhile(x => !char.IsLetterOrDigit(x) && x != '?' || char.IsWhiteSpace(x))
            .ToArray();
        var command = (new string(preamble)).ConvertToCommandType();
        var afterCommands = trimmedPayload.Substring(preamble.Length);

        result = null;
        if (!afterCommands.StartsWith("?"))
            return false;

        var cleanPayload = new string(afterCommands.Skip(1).TakeWhile(x => x != '*').ToArray());

        var mappings = afterCommands.Substring(1 + cleanPayload.Length)
            .Trim()
            .Split("*", StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim().Split("=>", StringSplitOptions.RemoveEmptyEntries).Select(y => y.Trim()).ToArray())
            .ToArray();

        if (mappings.Any(x => x.Length != 2))
            throw new ArgumentException($"Switching placeholder {cleanPayload} contained incomplete mapping");

        var expressionFilters = mappings.Select(BuildFilter).ToArray();
        result = new SwitchingFilter(cleanPayload.Trim(), command, expressionFilters);
        return true;
    }

    private static (Regex condition, ITemplateFilter filter) BuildFilter(string[] arg)
    {
        var condition = new Regex(arg[0]);

        if (!FormattingFilter.TryParse(arg[1], out var filter) &&
            !LiteralFilter.TryParse(arg[1], out filter) &&
            !PassthroughFilter.TryParse(arg[1], out filter) ||
            filter == null)
            throw new ArgumentException($"Could not parse filter '{arg[0]}'");

        return (condition, filter);
    }

    public string Render(object value)
    {
        var matches = Mappings.Where(x => x.Regex.IsMatch(Convert.ToString(value))).ToArray();
        return matches.Length > 0 ? matches[0].Filter.Render(value) : "";
    }
}