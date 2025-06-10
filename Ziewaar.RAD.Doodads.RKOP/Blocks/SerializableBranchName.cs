#nullable enable
using Ziewaar;
using Ziewaar.RAD.Doodads.RKOP.Text;

namespace Ziewaar.RAD.Doodads.RKOP.Blocks;
public class SerializableBranchName : IParityParser, IComparable, IComparable<SerializableBranchName>
{
    public string BranchName { get; private set; } = "";
    public ParityParsingState UpdateFrom(ref CursorText text)
    {
        text = text.SkipWhile(char.IsWhiteSpace).TakeToken(TokenDescription.Identifier, out var referenceIdentifier);
        if (!referenceIdentifier.IsValid)
            return ParityParsingState.Void;

        text = text.SkipWhile(char.IsWhiteSpace).ValidateToken(TokenDescription.BranchAnnouncement, out var assignment);

        if (string.IsNullOrWhiteSpace(BranchName))
        {
            BranchName = referenceIdentifier.Text;
            return ParityParsingState.New;
        }
        else if (BranchName != referenceIdentifier.Text)
        {
            BranchName = referenceIdentifier.Text;
            return ParityParsingState.Changed;
        }
        else
        {
            return ParityParsingState.Unchanged;
        }
    }
    public int CompareTo(object obj) =>
        obj is SerializableBranchName sbn
            ? string.Compare(BranchName, sbn.BranchName, StringComparison.Ordinal)
            : 1;
    public int CompareTo(SerializableBranchName other) =>
        string.Compare(BranchName, other.BranchName, StringComparison.Ordinal);
}