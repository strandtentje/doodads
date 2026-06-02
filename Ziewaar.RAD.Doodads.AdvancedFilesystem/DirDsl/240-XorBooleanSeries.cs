namespace Ziewaar.RAD.Doodads.AdvancedFilesystem.DirDsl;

public class XorBooleanSeries(IReverseFileExpression[] expressions) : IReverseFileExpression
{
    public IReverseFileExpression[] Expressions => expressions;
    public override string ToString()
    {
        return $"or-operation on {expressions.Length} expressions";
    }
    public bool Evaluate(string input, List<string> reasons)
    {
        bool xorIdentity = false;

        for (int i = 0; i < expressions.Length; i++)
        {
            xorIdentity ^= expressions[i].Evaluate(input, reasons);
            reasons.Add($"of {this}, member {i} flipped to {xorIdentity}");
        }

        reasons.Add($"of {this} xor result is {xorIdentity}");
        return xorIdentity;
    }
}
