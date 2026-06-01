using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace Ziewaar.RAD.Doodads.AdvancedFilesystem.DirDsl;

public class ComparableExpression(int numericValue) : IComparableExpression
{
    public static bool TryParseFrom(string name, List<(int c, string d)> errors, ref int cursor,
        bool requireString, [NotNullWhen(true)] out IComparableExpression? expression)
    {
        switch (name.ElementAtOrDefault(cursor))
        {
            case '`':
                if (requireString)
                {
                    expression = null;
                    errors.Add((cursor, "operator expects a string, not a number"));
                    return false;
                }

                cursor++;
                StringBuilder numeric = new();
                for (;
                     char.IsNumber(name.ElementAtOrDefault(cursor)) ||
                     name.ElementAtOrDefault(cursor) == '-';
                     cursor++)
                    numeric.Append(name[cursor]);
                if (name.ElementAtOrDefault(cursor++) != '`')
                {
                    cursor--;
                    expression = null;
                    errors.Add((cursor, "expected numeric closing ` or number"));
                    return false;
                }
                else if (!int.TryParse(numeric.ToString(), CultureInfo.InvariantCulture,
                             out int numericValue))
                {
                    expression = null;
                    errors.Add((cursor, "value between backticks wasnt a number"));
                    return false;
                }
                else
                {
                    expression = new ComparableExpression(numericValue);
                    return true;
                }
            case '\'':
                cursor++;
                StringBuilder textual = new();
                char c;
                while (true)
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
                else
                {
                    c = name.ElementAtOrDefault(cursor);
                    if (c == '%')
                    {
                        cursor++;
                        expression = new StringComparableExpression(textual.ToString(),
                            StringComparison.Ordinal);
                    }
                    else
                    {
                        expression = new StringComparableExpression(textual.ToString(),
                            StringComparison.OrdinalIgnoreCase);
                    }

                    return true;
                }
            default:
                errors.Add((cursor, "expected `numeric` or 'textual' expression"));
                expression = null;
                return false;
        }
    }
        
    public bool TryCompare(string other, out int relative)
    {
        if (!int.TryParse(other, out var otherInt))
        {
            relative = 0;
            return false;
        }
        else
        {
            relative = otherInt.CompareTo(numericValue);
            return true;
        }
    }

    public string Literal => numericValue.ToString();
    public StringComparison StringComparison { get; } = StringComparison.OrdinalIgnoreCase;

    public override string ToString()
    {
        return $"numeric [{numericValue}]";
    }
}