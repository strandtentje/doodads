using static System.String;

namespace Ziewaar.RAD.Doodads.AdvancedFilesystem.DirDsl;

public class StringComparableExpression(string text, StringComparison sc)
    : IComparableExpression
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="other"></param>
    /// <param name="relative"></param>
    /// <returns>Positive if the parameter is greater</returns>
    public bool TryCompare(string other, out int relative)
    {
        relative = Compare(other, text, sc);
        return true;
    }
    public string Literal => text;
    public StringComparison StringComparison => sc;
    public override string ToString()
    {
        return $"textual {text}";
    }
}