namespace Ziewaar.RAD.Doodads.AdvancedFilesystem.DirDsl;

public class GreaterInclusiveCondition(IComparableExpression operand)
    : ComparingCondition
{
    public override IComparableExpression Operand => operand;
    public override bool Evaluate(string input, List<string> reasons)
    {
        if (!operand.TryCompare(input, out int relative))
        {
            reasons.Add($"Input was not comparable to {operand}");
            return false;
        }
        else if (relative >= 0)
        {
            reasons.Add($"Accepted because input was greater or equals to {operand}");
            return true;
        }
        else
        {
            reasons.Add($"Rejected because input was less than {operand}");
            return false;
        }
    }

    public override string ToString()
    {
        return $"Greater than {operand}";
    }
}