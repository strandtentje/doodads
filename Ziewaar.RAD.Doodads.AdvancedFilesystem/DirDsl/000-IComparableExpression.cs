namespace Ziewaar.RAD.Doodads.AdvancedFilesystem.DirDsl;

public interface IComparableExpression
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="other"></param>
    /// <param name="relative">Positive if parameter is greater than expression</param>
    /// <returns>True if comparison could be made</returns>
    bool TryCompare(string other, out int relative);
    string Literal { get; }
    StringComparison StringComparison { get; }
}