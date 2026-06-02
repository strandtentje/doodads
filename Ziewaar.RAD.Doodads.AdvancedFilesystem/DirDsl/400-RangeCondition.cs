using System.Diagnostics.CodeAnalysis;

namespace Ziewaar.RAD.Doodads.AdvancedFilesystem.DirDsl;

public class RangeCondition(
    char operation,
    IComparableExpression left,
    IComparableExpression right) : IReverseFileExpression
{
    public IComparableExpression LBound => left;
    public IComparableExpression UBound => right;

    public bool Evaluate(string input, List<string> reasons)
    {
        if (!left.TryCompare(input, out int lowerRelative))
        {
            reasons.Add($"Input was not comparable to lower bound {left}");
            return false;
        }
        else if (!right.TryCompare(input, out int higherRelative))
        {
            reasons.Add($"Input was not comparable to upper bound {right}");
            return false;
        }
        else if (lowerRelative < 0)
        {
            reasons.Add($"Rejected because input was before {left}");
            return false;
        }
        else if (higherRelative >= 0)
        {
            reasons.Add($"Rejected because input was at or after {right}");
            return false;
        }
        else
        {
            reasons.Add($"Accepted because input was between {left} and {right}");
            return true;
        }
    }

    public override string ToString()
    {
        return $"Range of [{left}..{right}]";
    }

    public static bool TryParseFrom(string name, List<(int c, string d)> errors, ref int cursor,
        [NotNullWhen(true)] out IReverseFileExpression? expression)
    {
        char operation = name.ElementAtOrDefault(cursor);
        switch (operation)
        {
            case '_':
                cursor++;
                if (!ComparableExpression.TryParseFrom(name, errors, ref cursor, false,
                        out var leftOperand))
                {
                    expression = null;
                    errors.Add((cursor, "expected left operand"));
                    return false;
                }
                else if (name.ElementAtOrDefault(cursor++) != ',')
                {
                    cursor--;
                    expression = null;
                    errors.Add((cursor, "missing comma in range (_x,y) operation"));
                    return false;
                }
                else if (!ComparableExpression.TryParseFrom(name, errors, ref cursor, false,
                             out var rightOperand))
                {
                    expression = null;
                    errors.Add((cursor, "expected right operand"));
                    return false;
                }
                else
                {
                    expression = new RangeCondition(operation, leftOperand, rightOperand);
                    return true;
                }
            default:
                expression = null;
                errors.Add((cursor, "expected underscore for range operation"));
                return false;
        }
    }
}