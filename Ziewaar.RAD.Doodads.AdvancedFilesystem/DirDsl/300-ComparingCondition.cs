using System.Diagnostics.CodeAnalysis;

namespace Ziewaar.RAD.Doodads.AdvancedFilesystem.DirDsl
{
    public class ComparingCondition(char operation, IComparableExpression operand) : IReverseFileExpression
    {
        public static bool TryParseFrom(string name, List<(int c, string d)> errors, ref int cursor,
            [NotNullWhen(true)] out IReverseFileExpression? expression)
        {
            char operation = name[cursor];
            switch (operation)
            {
                case '(':
                    cursor++;
                    if (!RangeCondition.TryParseFrom(name, errors, ref cursor, out expression))
                    {
                        errors.Add((cursor, "malformed range condition"));
                        expression = null;
                        return false;
                    }
                    else if (name[cursor] != ')')
                    {
                        errors.Add((cursor, "missing closing parenthesis"));
                        expression = null;
                        return false;
                    }
                    else
                    {
                        cursor++;
                        return true;
                    }
                case '+':
                case '-':
                case '~':
                    cursor++;
                    if (!ComparableExpression.TryParseFrom(name, errors, ref cursor, false, out var operand))
                    {
                        errors.Add((cursor, "malformed comparable expression"));
                        expression = null;
                        return false;
                    }
                    else
                    {
                        expression = new ComparingCondition(operation, operand);
                        return true;
                    }
                case '$':
                case ';':
                    cursor++;
                    if (!ComparableExpression.TryParseFrom(name, errors, ref cursor, true, out var strOperand))
                    {
                        errors.Add((cursor, "malformed string comparable expression"));
                        expression = null;
                        return false;
                    }
                    else
                    {
                        expression = new ComparingCondition(operation, strOperand);
                        return true;
                    }
                default:
                    if (!ComparableExpression.TryParseFrom(name, errors, ref cursor, false, out var plainOperand))
                    {
                        errors.Add((cursor, "no explicit operator, and no exact match operand"));
                        expression = null;
                        return false;
                    }
                    else
                    {
                        expression = new ComparingCondition('@', plainOperand);
                        return true;
                    }
            }
        }

        public bool Evaluate(string input, List<string> reasons)
        {
            throw new NotImplementedException();
        }
    }
}