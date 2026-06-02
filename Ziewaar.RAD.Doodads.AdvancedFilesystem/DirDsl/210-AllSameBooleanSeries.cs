namespace Ziewaar.RAD.Doodads.AdvancedFilesystem.DirDsl;

public class AllSameBooleanSeries(IReverseFileExpression[] expressions) : IReverseFileExpression
{    
    public IReverseFileExpression[] Expressions => expressions;
    public override string ToString()
    {
        return $"or-operation on {expressions.Length} expressions";
    }
    public bool Evaluate(string input, List<string> reasons)
    {
        var startingEx = expressions.ElementAtOrDefault(0);
        bool startingState = startingEx?.Evaluate(input, reasons) == true;

        for (int i = 1; i < expressions.Length; i++)
        {
            if (expressions[i].Evaluate(input, reasons) != startingState)
            {
                reasons.Add($"of {this}, element {i} ({expressions[i]}) mismatched starting element {startingEx}");
                return false;
            }
        }

        reasons.Add($"of {this}, all evaluations had thesame output");
        return true;
    }
}
