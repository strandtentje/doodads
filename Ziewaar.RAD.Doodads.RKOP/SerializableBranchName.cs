#nullable enable
namespace Ziewaar.RAD.Doodads.RKOP;
public class SerializableBranchName : IParityParser, IComparable, IComparable<SerializableBranchName>
{
    public string BranchName { get; private set; } = "";
    public ParityParsingState UpdateFrom(ref CursorText text)
    {
        text = text.SkipWhile(char.IsWhiteSpace).TakeToken(TokenDescription.Identifier, out var referenceIdentifier);
        if (!referenceIdentifier.IsValid)
            return ParityParsingState.Void;

        text = text.SkipWhile(char.IsWhiteSpace).ValidateToken(TokenDescription.BranchAnnouncement, out var assignment);

        if (string.IsNullOrWhiteSpace(this.BranchName))
        {
            this.BranchName = referenceIdentifier.Text;
            return ParityParsingState.New;
        }
        else if (this.BranchName != referenceIdentifier.Text)
        {
            this.BranchName = referenceIdentifier.Text;
            return ParityParsingState.Changed;
        }
        else
        {
            return ParityParsingState.Unchanged;
        }
    }
    public int CompareTo(object obj) =>
        obj is SerializableBranchName sbn
            ? String.Compare(this.BranchName, sbn.BranchName, StringComparison.Ordinal)
            : 1;
    public int CompareTo(SerializableBranchName other) =>
        String.Compare(this.BranchName, other.BranchName, StringComparison.Ordinal);
}