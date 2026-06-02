namespace Ziewaar.RAD.Doodads.AdvancedFilesystem.DirDsl;

public class AndBooleanSeries(IReverseFileExpression[] expressions) : IReverseFileExpression
{    
    public IReverseFileExpression[] Expressions => expressions;
    public override string ToString()
    {
        return $"or-operation on {expressions.Length} expressions";
    }
    public bool Evaluate(string input, List<string> reasons)
    {
        foreach (var item in expressions)
        {
            if (!item.Evaluate(input, reasons))
            {
                reasons.Add($"of {this}, input didn't match {item}");
                return false;
            }
        }
        reasons.Add($"of {this}, input matched all");
        return true;
    }
}
