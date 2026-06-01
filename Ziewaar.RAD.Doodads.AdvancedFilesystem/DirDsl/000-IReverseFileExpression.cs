namespace Ziewaar.RAD.Doodads.AdvancedFilesystem.DirDsl
{
    public interface IReverseFileExpression
    {
        bool Evaluate(string input, List<string> reasons);
    }
}