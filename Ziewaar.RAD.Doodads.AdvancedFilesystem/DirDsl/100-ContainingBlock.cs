namespace Ziewaar.RAD.Doodads.AdvancedFilesystem.DirDsl;

public static class ContainingBlock
{
    public static bool TryParseFrom(string name, List<(int c, string d)> errors, ref int cursor, out IReverseFileExpression? expression)
    {
        switch (name[cursor])
        {
            case '[':
                cursor++;
                var contentsOk = BooleanSeries.TryParseFrom(name, errors, ref cursor, out expression);
                var closerOk = name.ElementAtOrDefault(cursor) == ']';
                if (!contentsOk) errors.Add((cursor, "Block contents wrong"));
                if (!closerOk) errors.Add((cursor, "Missing closing ]"));
                cursor++;
                return contentsOk && closerOk;
            default:
                if (!ComparingCondition.TryParseFrom(name, errors, ref cursor, out expression))
                {
                    errors.Add((cursor, "expected containing block [] or plain expression"));
                    expression = null;
                    return false;
                }
                else
                {
                    return true;
                }
        }
    }
}