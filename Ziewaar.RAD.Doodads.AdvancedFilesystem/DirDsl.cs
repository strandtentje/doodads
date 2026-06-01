using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

namespace Ziewaar.RAD.Doodads.AdvancedFilesystem
{
    public class DirDsl : IService
    {
        public event CallForInteraction? OnThen;
        public event CallForInteraction? OnElse;
        public event CallForInteraction? OnException;

        public void Enter(StampedMap constants, IInteraction interaction)
        {
            throw new NotImplementedException();
        }

        public void HandleFatal(IInteraction source, Exception ex)
        {
            throw new NotImplementedException();
        }
    }

    public interface IReverseFileExpression
    {
        bool Evaluate(string input, List<string> reasons);
    }

    public class ContainingBlock : IReverseFileExpression
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
                    var contentsOk = ComparingCondition.TryParseFrom(name, errors, ref cursor, out expression);


            }
        }

        public bool Evaluate(string input, List<string> reasons)
        {
            throw new NotImplementedException();
        }
    }

    public class BooleanSeries(char operation, IReverseFileExpression[] reverseFileExpressions) : IReverseFileExpression
    {
        public static bool TryParseFrom(string name, List<(int c, string d)> errors, ref int cursor,
            [NotNullWhen(true)] out IReverseFileExpression? expression)
        {
            char operation = name[cursor];
            switch (operation)
            {
                case '.':
                case '=':
                case '&':
                    bool partOk;
                    List<IReverseFileExpression> parts = new();
                    do
                    {
                        cursor++;
                        partOk = ContainingBlock.TryParseFrom(name, errors, ref cursor, out var part);
                        if (partOk) parts.Add(part);
                    } while (partOk && name[cursor] == ',');

                    if (!partOk)
                    {
                        errors.Add((cursor, "Invalid boolean series member"));
                        expression = null;
                        return false;
                    }
                    else
                    {
                        expression = new BooleanSeries(operation, parts.ToArray());
                        return true;
                    }
                default:
                    errors.Add((cursor, "Expected period, equals or ampersand as opener of []"));
                    expression = null;
                    return false;
            }
        }

        public bool Evaluate(string input, List<string> reasons)
        {
            throw new NotImplementedException();
        }
    }

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
                    if (!ComparableExpression.TryParseFrom(name, errors, ref cursor, out var operand))
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
                    if (!StringComparableExpression.TryParseFrom(name, errors, ref cursor, out var strOperand))
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
                    if (!ComparableExpression.TryParseFrom(name, errors, ref cursor, out var plainOperand))
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

    public class RangeCondition(char operation, IComparableExpression left, IComparableExpression right) : IReverseFileExpression
    {
        public static bool TryParseFrom(string name, List<(int c, string d)> errors, ref int cursor,
            [NotNullWhen(true)] out IReverseFileExpression? expression)
        {
            char operation = name.ElementAtOrDefault(cursor);
            switch (operation)
            {
                case '_':
                    cursor++;
                    if (!ComparableExpression.TryParseFrom(name, errors, ref cursor, out var leftOperand))
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
                    else if (!ComparableExpression.TryParseFrom(name, errors, ref cursor, out var rightOperand))
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

        public bool Evaluate(string input, List<string> reasons)
        {
            throw new NotImplementedException();
        }
    }

    public interface IComparableExpression
    {

    }

    public class ComparableExpression(int numericValue) : IComparableExpression
    {
        public static bool TryParseFrom(string name, List<(int c, string d)> errors, ref int cursor,
            [NotNullWhen(true)] out IComparableExpression? expression)
        {
            switch (name.ElementAtOrDefault(cursor))
            {
                case '`':
                    cursor++;
                    StringBuilder numeric = new();
                    for (; char.IsNumber(name.ElementAtOrDefault(cursor)) || name.ElementAtOrDefault(cursor) == '-'; cursor++)
                        numeric.Append(name[cursor]);
                    if (name.ElementAtOrDefault(cursor++) != '`')
                    {
                        cursor--;
                        expression = null;
                        errors.Add((cursor, "expected numeric closing ` or number"));
                        return false;
                    }
                    else if (!int.TryParse(numeric.ToString(), CultureInfo.InvariantCulture, out int numericValue))
                    {
                        expression = null;
                        errors.Add((cursor, "value between backticks wasnt a number"));
                        return false;
                    } else
                    {
                        expression = new ComparableExpression(numericValue);
                        return true;
                    }
                case '\'':
                    cursor++;
                    StringBuilder textual = new();
                    char c;
                    while(true)
                    {
                        c = name.ElementAtOrDefault(cursor++);
                        if (c != '\'' && c != default(char))
                            textual.Append(c);
                        else
                            break;
                    }
                    if (c != '\'')
                    {
                        expression = null;
                        errors.Add((cursor, "expected closing single quote"));
                        return false;
                    }
            }
        }

    }

    public class StringComparableExpression
    {
        public static bool TryParseFrom(string name, List<(int c, string d)> errors, ref int cursor,
            [NotNullWhen(true)] out IComparableExpression? expression)
        {

        }

    }