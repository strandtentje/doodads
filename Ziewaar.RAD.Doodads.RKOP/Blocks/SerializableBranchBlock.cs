#nullable enable
using System.Runtime.CompilerServices;
using Ziewaar;
using Ziewaar.RAD.Doodads.CoreLibrary;
using Ziewaar.RAD.Doodads.RKOP.SeriesParsers;
using Ziewaar.RAD.Doodads.RKOP.Text;

namespace Ziewaar.RAD.Doodads.RKOP.Blocks;

public class CompoundKey : IComparable, IComparable<CompoundKey>, IEquatable<CompoundKey>
{
    public readonly string[] Members;
    public CompoundKey(string[] members) => this.Members = [.. members.OrderBy(x => x)];
    public int CompareTo(CompoundKey other) =>
        string.CompareOrdinal(string.Join(", ", Members), string.Join(", ", other.Members));
    public int CompareTo(object obj) =>
        obj is CompoundKey ck ?
        CompareTo(ck) :
        throw new InvalidCastException("may only compare compound keys to other compound keys");
    public bool Equals(CompoundKey other) =>
        Enumerable.SequenceEqual(Members, other.Members);

}

public class SerializableBranchBlock<TResultSink>
    where TResultSink : class, IInstanceWrapper, new()
{
    public SortedList<string, ServiceExpression<TResultSink>> Convert()
    {
        SortedList<string, ServiceExpression<TResultSink>> result = new();
        if (Branches == null) return result;
        foreach (var item in Branches)
        {
            foreach (var key in item.Item1.Members)
            {
                if (result.ContainsKey(key))
                    GlobalLog.Instance?.Warning("Duplicate branch definition for {name}; using last.", key);
                result[key] = item.value;
            }
        }
        return result;
    }
    public List<(CompoundKey, ServiceExpression<TResultSink> value)>? Branches;
    private void GetWorkingSet(
        out List<(CompoundKey key, ServiceExpression<TResultSink> value)> set,
        out SortedSet<CompoundKey> key)
    {
        if (Branches == null)
        {
            set = Branches = new();
            key = [.. set.Select(x => x.key)];
        }
        else
        {
            set = Branches;
            key = [.. set.Select(x => x.key)]; ;
        }
    }
    private CursorText RecurseThroughBranches(
        CursorText text,
        List<(CompoundKey key, ServiceExpression<TResultSink> value)> branches,
        SortedSet<CompoundKey> toRemove)
    {
        var seenKey = new SerializableBranchName();
        if (seenKey.UpdateFrom(ref text) == ParityParsingState.Void)
        {
            // we couldnt parse a key so that means we'll be removing the remainder of the items
            // we previously worked on.
            foreach (var key in toRemove)
            {
                branches.Single(x => x.key.Equals(key)).value.Purge();
                branches.RemoveAll(x => x.key.Equals(key));
            }
            return text;
        }
        var detectedCompound = new CompoundKey(seenKey.BranchNames);
        if (branches.SingleOrDefault(x => x.key.Equals(detectedCompound)).value
            is ServiceExpression<TResultSink> serviceExpression)
        {
            // the branch key we detected corresponds to a branch compound we've parsed previously
            // and is still working. as such, we stop scheduling it for disposal and re-use what exists.
            toRemove.Remove(detectedCompound);
        }
        else
        {
            // we did not parse a branch with this name previously so we need to lay the groundwork
            // for a new servicve branch
            serviceExpression = new UnconditionalSerializableServiceSeries<TResultSink>();
            branches.Add((detectedCompound, serviceExpression));
        }
        var scopeName = seenKey.BranchNames.Length == 1 ?
            seenKey.BranchNames[0] :
            $"<-{string.Join(", ", seenKey.BranchNames)}->";
        serviceExpression.UpdateFrom(scopeName, ref text);

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
        text = text.SkipWhitespace().TakeToken(TokenDescription.BlockOpen, out var openBlock);

        if (openBlock.IsValid)
        {
            GetWorkingSet(out var workingSet, out var purgeKeys);
            text = RecurseThroughBranches(text.EnterScope(), workingSet, purgeKeys);
            text = text.SkipWhitespace();
            text = text.ValidateToken(
                TokenDescription.BlockClose,
                "This may also happen if for example a semicolon was forgotten, or shorthands like :? were accidentally typed as ?:",
                out var _);
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

                if (branch.Item1.Equals(new CompoundKey(["OnThen", "OnElse"])))
                    writer.Write("÷ ");
                else if (branch.Item1.Equals(new CompoundKey(["OnThen"])))
                    writer.Write(": ");
                else if (branch.Item1.Equals(new CompoundKey(["OnElse"])))
                    writer.Write("| ");
                else if (branch.Item1.Members.Length > 1)
                    writer.Write("<-");                
                writer.Write(string.Join(" ",branch.Item1.Members));
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