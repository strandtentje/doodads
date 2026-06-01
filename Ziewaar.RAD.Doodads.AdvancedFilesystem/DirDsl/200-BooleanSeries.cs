using System.Diagnostics.CodeAnalysis;

namespace Ziewaar.RAD.Doodads.AdvancedFilesystem.DirDsl
{
    public class BooleanSeries(char operation, IReverseFileExpression[] expressions) : IReverseFileExpression
    {
        public char Operation => operation;
        public IReverseFileExpression[] Expressions => expressions;
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

        public override string ToString()
        {
            return $"[{operation}]-operation on {expressions.Length} expressions";
        }

        public bool Evaluate(string input, List<string> reasons)
        {
            switch(operation)
            {
                case '.':
                    foreach (var item in expressions)
                    {
                        if (item.Evaluate(input, reasons))
                        {
                            reasons.Add($"of {this.ToString()}, input matched {item.ToString()}");
                            return true;
                        }
                    }
                    reasons.Add($"of {this.ToString()}, input matched nothing");
                    return false;
                case '&':
                    foreach (var item in expressions)
                    {
                        if (!item.Evaluate(input, reasons))
                        {
                            reasons.Add($"of {this.ToString()}, input didn't match {item.ToString()}");
                            return false;
                        }
                    }
                    reasons.Add($"of {this.ToString()}, input matched all");
                    return false;
                case '=':
                    var startingEx = expressions.ElementAtOrDefault(0);
                    bool startingState = startingEx?.Evaluate(input, reasons) == true;

                    for (int i = 1; i < expressions.Length; i++)
                    {
                        if (expressions[i].Evaluate(input, reasons) != startingState)
                        {
                            reasons.Add($"of {this.ToString()}, element {i} ({expressions[i].ToString()}) mismatched starting element {startingEx}");
                            return false;
                        }
                    }

                    reasons.Add($"of {this.ToString()}, all evaluations had thesame output");
                    return true;
                default:
                    reasons.Add($"unknown operator on {this.ToString()}");
                    return false;
            }
        }
    }
}