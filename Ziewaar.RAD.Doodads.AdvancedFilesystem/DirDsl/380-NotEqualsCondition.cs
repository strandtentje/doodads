namespace Ziewaar.RAD.Doodads.AdvancedFilesystem.DirDsl;

public class NotEqualsCondition(IComparableExpression operand)
    : ComparingCondition
{
    public override IComparableExpression Operand => operand;
    public override bool Evaluate(string input, List<string> reasons)
    {
        if (!operand.TryCompare(input, out int relative))
        {
            reasons.Add($"Input was not comparable to {operand}");
            return true;
        }
        else if (relative != 0)
        {
            reasons.Add($"Accepted because input was not equal to {operand}");
            return true;
        }
        else
        {
            reasons.Add($"Rejected because input was equal to {operand}");
            return false;
        }
    }

    public override string ToString()
    {
        return $"Not equal to [{operand}]";
    }
}