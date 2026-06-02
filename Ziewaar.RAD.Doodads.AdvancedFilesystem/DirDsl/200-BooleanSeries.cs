using System.Diagnostics.CodeAnalysis;

namespace Ziewaar.RAD.Doodads.AdvancedFilesystem.DirDsl;

public static class BooleanSeries
{
    public static bool TryParseFrom(string name, List<(int c, string d)> errors, ref int cursor,
        [NotNullWhen(true)] out IReverseFileExpression? expression)
    {
        char operation = name.ElementAtOrDefault(cursor);
        switch (operation)
        {
            case '.':
            case '=':
            case '&':
            case '@':
                bool partOk;
                List<IReverseFileExpression> parts = new();
                do
                {
                    cursor++;
                    partOk = ContainingBlock.TryParseFrom(name, errors, ref cursor, out var part);
                    if (partOk && part != null) parts.Add(part);
                } while (partOk && name.ElementAtOrDefault(cursor) == ',');

                if (!partOk)
                {
                    errors.Add((cursor, "Invalid boolean series member"));
                    expression = null;
                    return false;
                }
                else
                {
                    expression = operation switch
                    {
                        '.' => new OrBooleanSeries(parts.ToArray()),
                        '=' => new AllSameBooleanSeries(parts.ToArray()),
                        '&' => new AndBooleanSeries(parts.ToArray()),
                        '@' => new XorBooleanSeries(parts.ToArray()),
                        _ => throw new InvalidOperationException("unsupported operator")
                    };
                    return true;
                }
            default:
                errors.Add((cursor, "Expected period, equals or ampersand as opener of []"));
                expression = null;
                return false;
        }
    }
}