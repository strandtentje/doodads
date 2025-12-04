#nullable enable
using System.Buffers;
using Ziewaar;
using Ziewaar.RAD.Doodads.RKOP.Text;

namespace Ziewaar.RAD.Doodads.RKOP.Blocks;
public class SerializableBranchName : IParityParser, IComparable, IComparable<SerializableBranchName>
{
    public string[] BranchNames { get; private set; } = [];
    public ParityParsingState UpdateFrom(ref CursorText text)
    {
        text = text.SkipWhitespace().
            TakeToken(TokenDescription.OnThenShorthand, out var onThenShorthand).
            TakeToken(TokenDescription.OnElseShorthand, out var onElseShorthand).
            TakeToken(TokenDescription.OnEitherShorthand, out var onEitherShorthand).
            TakeToken(TokenDescription.CompoundBranchAnnouncement, out var onMany);

        switch ((onThenShorthand.IsValid, onElseShorthand.IsValid, onEitherShorthand.IsValid, onMany.IsValid))
        {
            case (true, false, false, false): 
                return ChangeFor(["OnThen"]);
            case (false, true, false, false):
                return ChangeFor(["OnElse"]);
            case (false, false, true, false):
                return ChangeFor(["OnThen", "OnElse"]);
            case (false, false, false, true):
                List<string> names = new();
                for (text = text.
                    SkipWhitespace().
                    TakeToken(TokenDescription.Identifier, out var compoundIdentifierMember);
                    compoundIdentifierMember.IsValid;
                    text = text.
                    SkipWhitespace().
                    TakeToken(TokenDescription.ArgumentSeparator, out var argumentSeparator))
                    names.Add(compoundIdentifierMember.Text);

                text = text.TakeToken(TokenDescription.BranchAnnouncement, out var endOfArgs);
                if (!endOfArgs.IsValid)
                    throw new SyntaxException(text, "expected closing bracket -> at end of compound branch names");

                return ChangeFor([.. names]);
            default:
                text = text.SkipWhitespace().TakeToken(TokenDescription.Identifier, out var referenceIdentifier);
                if (!referenceIdentifier.IsValid)
                    return ParityParsingState.Void;

                text = text.SkipWhitespace().ValidateToken(
                    TokenDescription.BranchAnnouncement, 
                    "only use the -> coupler here, and the dash wasn't forgotten. other operators aren't allowed.",
                    out var assignment);

                return ChangeFor([referenceIdentifier.Text]);
        }        
    }
    private ParityParsingState ChangeFor(string[] newBranchNames)
    {
        if (BranchNames.Length == 0)
        {
            BranchNames = newBranchNames;
            return Text.ParityParsingState.New;
        } else if (!BranchNames.OrderBy(x => x).SequenceEqual(newBranchNames.OrderBy(x => x)))
        {
            BranchNames = newBranchNames;
            return Text.ParityParsingState.Changed;
        }
        else
        {
            return Text.ParityParsingState.Unchanged;
        }
    }
    public int CompareTo(object obj) =>
        obj is SerializableBranchName sbn
            ? string.Compare(string.Join(", ", BranchNames), string.Join(", ", sbn.BranchNames), StringComparison.Ordinal)
            : 1;
    public int CompareTo(SerializableBranchName other) =>
        string.Compare(string.Join(", ", BranchNames.OrderBy(x => x)), string.Join(", ", other.BranchNames.OrderBy(x => x)), StringComparison.Ordinal);
}