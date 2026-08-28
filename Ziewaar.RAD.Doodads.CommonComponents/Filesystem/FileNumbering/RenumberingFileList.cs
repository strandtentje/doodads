#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.CommonComponents.Filesystem.FileNumbering;

public class RenumberingFileList(FileInfo? reorderFile, int direction) : List<RenumberingFile>
{
    private readonly List<RenumberingFile> Files = new();
    private readonly Denominator Denom = new Denominator(0);

    private RenumberingFile? ExchangesNumberWithNextMember = null;

    public RenumberingFileList Append(FileInfo newFileItem)
    {
        Denom.Increment();
        var newMember = new RenumberingFile(newFileItem, new Fraction(Denom.Value, Denom));
        if (this.ExchangesNumberWithNextMember != null)
        {
            Files.Remove(ExchangesNumberWithNextMember);

            Files.Add(new RenumberingFile(newMember.InfoBeforeRename, ExchangesNumberWithNextMember.ProposedNumber));
            Files.Add(new RenumberingFile(ExchangesNumberWithNextMember.InfoBeforeRename, newMember.ProposedNumber));

            this.ExchangesNumberWithNextMember = null;
        }
        else if (reorderFile?.FullName == newFileItem.FullName && direction == -1 &&
            Files.LastOrDefault() is RenumberingFile previouslyAddedMember)
        {
            Files.Remove(previouslyAddedMember);

            Files.Add(new RenumberingFile(newMember.InfoBeforeRename, previouslyAddedMember.ProposedNumber));
            Files.Add(new RenumberingFile(previouslyAddedMember.InfoBeforeRename, newMember.ProposedNumber));
        }
        else if (reorderFile?.FullName == newFileItem.FullName && direction == 1)
        {
            this.ExchangesNumberWithNextMember = newMember;
            Files.Add(newMember);
        } else
        {
            Files.Add(newMember);
        }
        return this;
    }
    public RenumberingFile[] Build() => Files.ToArray();
}
