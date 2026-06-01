namespace Ziewaar.RAD.Doodads.AdvancedFilesystem.DirDsl;

public class EndsWithCondition(IComparableExpression operand)
    : ComparingCondition
{
    public override bool Evaluate(string input, List<string> reasons)
    {
        if (input.EndsWith(operand.Literal, operand.StringComparison))
        {
            reasons.Add($"Accepted because input ended with {operand}");
            return true;
        }
        else
        {
            reasons.Add($"Rejected because input did not end with {operand}");
            return false;
        }
    }
    public override string ToString()
    {
        return $"Ends with {operand}";
    }
}