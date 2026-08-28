#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.CommonComponents.Filesystem.FileNumbering;

public class RenumberingFile(FileInfo info, Fraction fraction)
{
    public FileInfo InfoBeforeRename => info;
    #region Before Renumber
    public string OldPath => info.FullName;
    public string OldName => info.Name;
    public string WithoutNumber
    {
        get
        {
            if (field != null) return field;
            field = new string(OldName.SkipWhile(char.IsNumber).SkipWhile(char.IsSymbol).ToArray()).Trim().Trim('-').Trim();
            return field;
        }
    }
    #endregion
    #region After Renumber
    public Fraction ProposedNumber => fraction;
    public string IntermediateName => field ??= $"inter-{fraction.Numerator}-{fraction.Denom.Value}-{Guid.NewGuid().ToString()}.inter.move";
    public string IntermediatePath => Path.Combine(info.Directory.FullName, IntermediateName);
    public string FinalName => $"{fraction.NumberPrefix.ToString(fraction.Denom.Format)}-{WithoutNumber}";
    public string FinalPath => Path.Combine(info.Directory.FullName, FinalName);
    #endregion
    public bool IsRenameNeeded => OldName != FinalName;
}
