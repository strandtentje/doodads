#nullable enable
using Ziewaar;
using Ziewaar.RAD.Doodads.RKOP.SeriesParsers;
using Ziewaar.RAD.Doodads.RKOP.Text;

namespace Ziewaar.RAD.Doodads.RKOP.Blocks;
public class SerializableBranchBlock<TResultSink>
    where TResultSink : class, IInstanceWrapper, new()
{
    public List<(string key, ServiceExpression<TResultSink> value)>? Branches;
    private void GetWorkingSet(
        out List<(string key, ServiceExpression<TResultSink> value)> set,
        out SortedSet<string> keys)
    {
        if (Branches == null)
        {
            set = Branches = new();
            keys = [.. set.Select(x => x.key)];
        }
        else
        {
            set = Branches;
            keys = [.. set.Select(x => x.key)];;
        }
    }
    private CursorText RecurseThroughBranches(
        CursorText text,
        List<(string key, ServiceExpression<TResultSink> value)> branches,
        SortedSet<string> toRemove)
    {
        var seenKey = new SerializableBranchName();
        if (seenKey.UpdateFrom(ref text) == ParityParsingState.Void)
        {
            foreach (var key in toRemove)
            {
                branches.Single(x => x.key == key).value.Purge();
                branches.RemoveAll(x => x.key == key);
            }
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
        }
        serviceExpression.UpdateFrom(seenKey.BranchName, ref text);
        text = text.TakeToken(TokenDescription.Terminator, out var terminator);
        if (terminator.IsValid)
            // The alternative approach is a while(true) loop. 
            // ReSharper disable once TailRecursiveCall
            return RecurseThroughBranches(text, branches, toRemove);
        else
            return text;
    }
    public bool UpdateFrom(ref CursorText text)
    {
        text = text.SkipWhile(char.IsWhiteSpace).TakeToken(TokenDescription.BlockOpen, out var openBlock);

        if (openBlock.IsValid)
        {
            GetWorkingSet(out var workingSet, out var purgeKeys);
            text = RecurseThroughBranches(text.EnterScope(), workingSet, purgeKeys);
            text = text.SkipWhile(char.IsWhiteSpace);
            text = text.ValidateToken(TokenDescription.BlockClose, out var _);
            text = text.ExitScope();

            return true;
        }
        return false;
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
    public void Purge()
    {
        if (Branches?.Count > 0)
        {
            foreach (var valueTuple in Branches)
            {
                valueTuple.value.Purge();
            }
        }
    }
}