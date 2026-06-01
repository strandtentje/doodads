namespace Ziewaar.RAD.Doodads.AdvancedFilesystem.DirDsl;

public class LesserInclusiveCondition(IComparableExpression operand)
    : ComparingCondition
{
    public override bool Evaluate(string input, List<string> reasons)
    {
        if (!operand.TryCompare(input, out int relative))
        {
            reasons.Add($"Input was not comparable to {operand}");
            return false;
        }
        else if (relative < 0)
        {
            reasons.Add($"Accepted because Input was less than {operand}");
            return true;
        }
        else
        {
            reasons.Add($"Rejected because input was greater or equal to {operand}");
            return false;
        }
    }

    public override string ToString()
    {
        return $"Less or equal to {operand}";
    }
}