#nullable enable
namespace Ziewaar.RAD.Doodads.RKOP;
public class SerializableBranchBlock<TResultSink> : IParityParser
    where TResultSink : class, IInstanceWrapper, new()
{
    public SortedList<string, ServiceExpression<TResultSink>>? Branches;
    private ParityParsingState GetWorkingSet(
        out SortedList<string, ServiceExpression<TResultSink>> set,
        out SortedSet<string> keys)
    {
        if (Branches == null)
        {
            set = Branches = new();
            keys = new(set.Keys);
            return ParityParsingState.New;
        }
        else
        {
            set = Branches;
            keys = new(set.Keys);
            return ParityParsingState.Unchanged;
        }
    }
    private CursorText RecurseThroughBranches(
        CursorText text,
        SortedList<string, ServiceExpression<TResultSink>> branches,
        SortedSet<string> toRemove,
        ref ParityParsingState state)
    {
        var seenKey = new SerializableBranchName();
        if (seenKey.UpdateFrom(ref text) == ParityParsingState.Void)
        {
            foreach (var key in toRemove)
            {
                branches[key].Purge();
                branches.Remove(key);
            }
            state = branches.Count == 0 ? ParityParsingState.Void : state;
            return text;
        }
        if (branches.TryGetValue(seenKey.BranchName, out var serviceExpression))
        {
            toRemove.Remove(seenKey.BranchName);
        }
        else
        {
            serviceExpression = branches[seenKey.BranchName] = new UnconditionalSerializableServiceSeries<TResultSink>();
            state |= ParityParsingState.Changed;
        }
        state |= serviceExpression.UpdateFrom(seenKey.BranchName, ref text);
        text = text.TakeToken(TokenDescription.Terminator, out var terminator);
        if (terminator.IsValid)
            // The alternative approach is a while(true) loop. 
            // ReSharper disable once TailRecursiveCall
            return RecurseThroughBranches(text, branches, toRemove, ref state);
        else
            return text;
    }
    public ParityParsingState UpdateFrom(ref CursorText text)
    {
        text = text.SkipWhile(char.IsWhiteSpace).TakeToken(TokenDescription.BlockOpen, out var openBlock);

        if (openBlock.IsValid)
        {
            var state = GetWorkingSet(out var workingSet, out var purgeKeys);

            text = RecurseThroughBranches(text.EnterScope(), workingSet, purgeKeys, ref state)
                .SkipWhile(char.IsWhiteSpace)
                .ValidateToken(TokenDescription.BlockClose, out var _)
                .ExitScope();

            return state;
        }
        else if (Branches is { Count: > 0 })
        {
            foreach (var branch in Branches)
                branch.Value.Purge();
            Branches.Clear();
            Branches = null;
            return ParityParsingState.Void;
        }
        else
        {
            return ParityParsingState.Unchanged;
        }
    }
    public void WriteTo(StreamWriter writer, int indentation)
    {
        if (Branches?.Count > 0)
        {
            indentation += 4;
            writer.Write(" {");
            foreach (var branch in Branches)
            {
                writer.Write(new string(' ', indentation));
                writer.Write(branch.Key);
                writer.Write("->");
                branch.Value.WriteTo(writer, indentation);
                writer.WriteLine(";");
            }
            indentation -= 4;
            writer.Write(new string(' ', indentation));
            writer.Write("}");
        }
    }
}