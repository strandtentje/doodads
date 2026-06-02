namespace Ziewaar.RAD.Doodads.AdvancedFilesystem.DirDsl;

public class ContainsCondition(IComparableExpression operand)
    : ComparingCondition
{
    public override IComparableExpression Operand => operand;
    public override bool Evaluate(string input, List<string> reasons)
    {
        if (input.Contains(operand.Literal, operand.StringComparison))
        {
            reasons.Add($"Accepted because input started with {operand}");
            return true;
        }
        else
        {
            reasons.Add($"Rejected input did not start with {operand}");
            return false;
        }
    }

    public override string ToString()
    {
        return $"Starts with {operand}";
    }
}