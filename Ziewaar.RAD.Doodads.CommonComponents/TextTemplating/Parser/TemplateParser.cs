using System.Runtime.ConstrainedExecution;

namespace Ziewaar.RAD.Doodads.CommonComponents.TextTemplating.Parser;

public class TemplateParser
{
    private readonly List<TemplateCommand> CommandStack = new();
    private readonly HashSet<string> AvailableTrueLocales = new();
    public ISet<string> Locales => AvailableTrueLocales;
    public IReadOnlyList<TemplateCommand> TemplateCommands => CommandStack;
    public IEnumerable<IGrouping<string, TemplateCommand>> CommandsByLocale { get; private set; }

    public void RefreshTemplateData(StreamReader reader)
    {
        var allTemplateText = reader.ReadToEnd();
        RefreshTemplateData(allTemplateText);
        reader.Dispose();
    }

    public void RefreshTemplateData(string allTemplateText)
    {
        CommandStack.Clear();
        AvailableTrueLocales.Clear();
        string currentLocale = "*_*";
        for (int selectionHead = 0;
             selectionHead < allTemplateText.Length;
             selectionHead = FindNextHead(allTemplateText, selectionHead, ref currentLocale))
            if (currentLocale != "*_*")
                AvailableTrueLocales.Add(currentLocale.ToLowerInvariant());
        CommandsByLocale = CommandStack.GroupBy(x => x.Locale.ToLower().Replace('-', '_'));
    }

    private int FindNextHead(string allTemplateText, int cursor, ref string currentLocale)
    {
        (TemplateTokens.ControlType openingType, int openerPosition) = allTemplateText.FindNextControlStart(cursor);
        if (openerPosition < 0)
            openerPosition = allTemplateText.Length;
        CommandStack.Add(new()
        {
            Position = cursor,
            Locale = currentLocale,
            PayloadText = allTemplateText.Substring(cursor, openerPosition - cursor),
            Type = TemplateCommandType.LiteralSource,
        });
        (TemplateTokens.ControlType closingType, cursor) = allTemplateText.FindNextControlEnd(openerPosition);
        if (cursor < 0 || closingType != openingType)
        {
            if (openerPosition > 0 && openingType != TemplateTokens.ControlType.None)
            {
                throw new Exception(string.Format("The matching closer could not be found"));
            }

            return allTemplateText.Length;
        }

        var dirtyPayload = allTemplateText.Substring(
            openerPosition + TemplateTokens.PLACEHOLDER_START.Length,
            cursor - openerPosition - TemplateTokens.PLACEHOLDER_END.Length).Trim();

        switch (openingType)
        {
            case TemplateTokens.ControlType.Placeholder:
                var command = ParsePlaceholder(dirtyPayload, currentLocale, openerPosition);
                CommandStack.Add(command);
                break;
            case TemplateTokens.ControlType.Locale:
                var trimmedDirty = dirtyPayload.Trim();
                if (trimmedDirty == "*" || trimmedDirty == "default") trimmedDirty = "*_*";
                currentLocale = trimmedDirty;
                break;
            default:
                return allTemplateText.Length;
        }

        return cursor + TemplateTokens.PLACEHOLDER_END.Length;
    }

    private static TemplateCommand ParsePlaceholder(string dirtyPayload, string forLocale, int position)
    {
        if (!SwitchingFilter.TryParse(dirtyPayload, out var result) &&
            !CoalescingFilter.TryParse(dirtyPayload, out result) &&
            !LiteralFilter.TryParse(dirtyPayload, out result) &&
            !FormattingFilter.TryParse(dirtyPayload, out result) &&
            !PassthroughFilter.TryParse(dirtyPayload, out result) ||
            result == null)
            throw new ArgumentException($"Failed to parse placeholder #{position} named {dirtyPayload}");

        return new()
        {
            Position = position,
            Locale = forLocale,
            PayloadText = result.CleanPayload,
            Type = result.Command,
            Formatter = result.Render,
        };
    }
}