namespace Ziewaar.RAD.Doodads.CommonComponents.TextTemplating.Parser;

public class TemplateParser(string placeholderStart = "{% ", string placeholderEnd = " %}")
{
    private readonly List<TemplateCommand> CommandStack = new();
    public IReadOnlyList<TemplateCommand> TemplateCommands => CommandStack;
    public void RefreshTemplateData(StreamReader reader)
    {
        var allTemplateText = reader.ReadToEnd();
        CommandStack.Clear();
        for(int selectionHead = 0;
            selectionHead < allTemplateText.Length;
            selectionHead = FindNextHead(allTemplateText, selectionHead))
            ;
        reader.Dispose();
    }
    private int FindNextHead(string allTemplateText, int cursor)
    {
        var openerPosition = allTemplateText.OrdinalIndexOfPlaceholderStart(cursor);
        if (openerPosition < 0)
            openerPosition = allTemplateText.Length;
        CommandStack.Add(new()
        {
            PayloadText = allTemplateText.Substring(cursor, openerPosition - cursor),
            Type = TemplateCommandType.LiteralSource,
        });
        cursor = allTemplateText.OrdinalIndexOfPlaceholderEnd(openerPosition);
        if (cursor < 0)
            return allTemplateText.Length;

        var dirtyPayload = allTemplateText.Substring(
            openerPosition + placeholderStart.Length,
            cursor - openerPosition - placeholderEnd.Length).Trim();
        var command = ParsePlaceholder(dirtyPayload);

        CommandStack.Add(command);
        return cursor + placeholderEnd.Length;
    }
    private static TemplateCommand ParsePlaceholder(string dirtyPayload)
    {
        var colonPayload = dirtyPayload.TrimTemplateTokens();
        var modifiers = dirtyPayload.Substring(0, dirtyPayload.Length - colonPayload.Length);
        var splitPayload = colonPayload.Split([':'], count: 2);
        var cleanPayload = splitPayload.ElementAtOrDefault(0) ?? "";
        var formatter = splitPayload.ElementAtOrDefault(1) ?? "";
        TemplateCommand command = new()
        {
            PayloadText = cleanPayload,
            Type = modifiers.ElementAtOrDefault(0).ConvertToSourceCommandType() |
                   modifiers.ElementAtOrDefault(1).ConvertToFilterCommandType(),
            Formatter = string.IsNullOrWhiteSpace(formatter) ? null : (object o) =>
            {
                if (o.ConvertNumericToDecimal() is decimal decimalValue)
                    return decimalValue.ToString(formatter);
                else if (o is DateTime dateTimeValue)
                    return dateTimeValue.ToString(formatter);
                else if (o is TimeSpan timespanValue)
                    return timespanValue.ToString(formatter);
                else
                    return o.ToString();
            }
        };
        return command;
    }
}