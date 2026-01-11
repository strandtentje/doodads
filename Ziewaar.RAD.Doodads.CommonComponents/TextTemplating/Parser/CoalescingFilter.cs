namespace Ziewaar.RAD.Doodads.CommonComponents.TextTemplating.Parser;

public class CoalescingFilter : ITemplateFilter
{
    private CoalescingFilter(string cleanPayload, TemplateCommandType command, string alternative) =>
        (CleanPayload, Command, Alternative) = (cleanPayload, command, alternative);
    public string CleanPayload { get; }
    public readonly string Alternative;
    public TemplateCommandType Command { get; }

    public static bool TryParse(string dirtyPayload, out ITemplateFilter? result)
    {
        var trimmedPayload = dirtyPayload.Trim();
        var preamble = trimmedPayload.TakeWhile(x => !char.IsLetterOrDigit(x) && x != '!' || char.IsWhiteSpace(x))
            .ToArray();
        var command = (new string(preamble)).ConvertToCommandType();
        var afterCommands = trimmedPayload.Substring(preamble.Length);

        result = null;
        if (!afterCommands.StartsWith("!"))
            return false;

        var halves = afterCommands.Substring(1).Split("=>", StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
        if (halves.Length != 2)
            throw new ArgumentException("Coalescing filter requires a payload name, then `=>` then alternative expression");

        result = new CoalescingFilter(halves[0], command, halves[1]);
        return true;
    }

    public string Render(object value)
    {
        var converted = Convert.ToString(value);
        return string.IsNullOrWhiteSpace(converted) ? Alternative : converted;
    }
}