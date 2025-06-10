#nullable enable
namespace Ziewaar.RAD.Doodads.RKOP;
public class SerializableBranchBlock<TResultSink> : IParityParser
    where TResultSink : class, IInstanceWrapper, new()
{
    public List<(string key, ServiceExpression<TResultSink> value)>? Branches;
    private ParityParsingState GetWorkingSet(
        out List<(string key, ServiceExpression<TResultSink> value)> set,
        out SortedSet<string> keys)
    {
        if (Branches == null)
        {
            set = Branches = new();
            keys = [.. set.Select(x => x.key)];
            return ParityParsingState.New;
        }
        else
        {
            set = Branches;
            keys = [.. set.Select(x => x.key)];
            return ParityParsingState.Unchanged;
        }
    }
    private CursorText RecurseThroughBranches(
        CursorText text,
        List<(string key, ServiceExpression<TResultSink> value)> branches,
        SortedSet<string> toRemove,
        ref ParityParsingState state)
    {
        var seenKey = new SerializableBranchName();
        if (seenKey.UpdateFrom(ref text) == ParityParsingState.Void)
        {
            foreach (var key in toRemove)
            {
                branches.Single(x => x.key == key).value.Purge();
                branches.RemoveAll(x => x.key == key);
            }
            state = branches.Count == 0 ? ParityParsingState.Void : state;
            return text;
        }
        if (branches.SingleOrDefault(x => x.key == seenKey.BranchName).value is ServiceExpression<TResultSink> serviceExpression)
        {
            toRemove.Remove(seenKey.BranchName);
        }
        else
        {
            serviceExpression = new UnconditionalSerializableServiceSeries<TResultSink>();
            branches.Add((seenKey.BranchName, serviceExpression));
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

            text = RecurseThroughBranches(text.EnterScope(), workingSet, purgeKeys, ref state);
            text = text.SkipWhile(char.IsWhiteSpace);
            text = text.ValidateToken(TokenDescription.BlockClose, out var _);
            text = text.ExitScope();

            return state;
        }
        else if (Branches is { Count: > 0 })
        {
            foreach (var branch in Branches)
                branch.value.Purge();
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
            writer.WriteLine(" {");
            foreach (var branch in Branches)
            {
                writer.Write(new string(' ', indentation));
                writer.Write(branch.key);
                writer.Write("->");
                branch.value.WriteTo(writer, indentation);
                writer.WriteLine(";");
            }
            indentation -= 4;
            writer.Write(new string(' ', indentation));
            writer.Write("}");
        }
    }
}