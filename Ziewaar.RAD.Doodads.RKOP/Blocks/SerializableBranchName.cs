#nullable enable
using Ziewaar;
using Ziewaar.RAD.Doodads.RKOP.Text;

namespace Ziewaar.RAD.Doodads.RKOP.Blocks;
public class SerializableBranchName : IParityParser, IComparable, IComparable<SerializableBranchName>
{
    public string BranchName { get; private set; } = "";
    public ParityParsingState UpdateFrom(ref CursorText text)
    {
        text = text.SkipWhile(char.IsWhiteSpace).
            TakeToken(TokenDescription.OnThenShorthand, out var onThenShorthand).
            TakeToken(TokenDescription.OnElseShorthand, out var onElseShorthand);

        if (onThenShorthand.IsValid && !onElseShorthand.IsValid)
            return ChangeFor("OnThen");
        else if (!onThenShorthand.IsValid && onElseShorthand.IsValid)
            return ChangeFor("OnElse");
        else if (onThenShorthand.IsValid && onElseShorthand.IsValid)
            throw new SyntaxException(text, "can't use both . and ,");
        
        text = text.SkipWhile(char.IsWhiteSpace).TakeToken(TokenDescription.Identifier, out var referenceIdentifier);
        if (!referenceIdentifier.IsValid)
            return ParityParsingState.Void;

        text = text.SkipWhile(char.IsWhiteSpace).ValidateToken(
            TokenDescription.BranchAnnouncement, 
            "only use the -> coupler here, and the dash wasn't forgotten. other operators aren't allowed.",
            out var assignment);

        return ChangeFor(referenceIdentifier.Text);
    }
    private ParityParsingState ChangeFor(string newBranchName)
    {
        if (string.IsNullOrWhiteSpace(BranchName))
        {
            BranchName = newBranchName;
            return Text.ParityParsingState.New;
        } else if (BranchName != newBranchName)
        {
            BranchName = newBranchName;
            return Text.ParityParsingState.Changed;
        }
        else
        {
            return Text.ParityParsingState.Unchanged;
        }
    }
    public int CompareTo(object obj) =>
        obj is SerializableBranchName sbn
            ? string.Compare(BranchName, sbn.BranchName, StringComparison.Ordinal)
            : 1;
    public int CompareTo(SerializableBranchName other) =>
        string.Compare(BranchName, other.BranchName, StringComparison.Ordinal);
}