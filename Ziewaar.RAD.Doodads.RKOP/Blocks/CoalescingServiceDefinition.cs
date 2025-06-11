#nullable enable

using Ziewaar;
using Ziewaar.RAD.Doodads.RKOP.Text;

namespace Ziewaar.RAD.Doodads.RKOP.Blocks;
public class CoalescingServiceDefinition<TResultSink> :
    ServiceExpression<TResultSink>
    where TResultSink : class, IInstanceWrapper, new()
{
    private SerializableRedirection<TResultSink>? Redirection;
    private ServiceDescription<TResultSink>? Description;
    protected override bool ProtectedUpdateFrom(ref CursorText text)
    {
        ResultSink ??= new();
        Redirection ??= new();
        if (Redirection.UpdateFrom($"r_{CurrentNameInScope}", ref text))
            return true;
        else
            Redirection = null;

        Description ??= new();
        if (Description.UpdateFrom($"d_{CurrentNameInScope}", ref text))
            return true;
        Description = null;
        return false;
    }
    public override void HandleChanges()
    {
        if (ResultSink == null)
            throw new ArgumentException("no result sink", nameof(ResultSink));
        else if (Redirection != null)
            ResultSink.SetSoftLink(Redirection);
        else if (Description != null)
            ResultSink.SetHardLink(Description);
        else
            throw new ArgumentException("no redirection or description");
    }
    public override void Purge()
    {
        ResultSink?.Cleanup();
        Redirection?.Purge();
        Description?.Purge();
        Redirection = null;
        Description = null;
    }
    public override void WriteTo(StreamWriter writer, int indentation = 0)
    {
        if (Redirection != null)
            Redirection.WriteTo(writer, indentation);
        else if (Description != null)
            Description.WriteTo(writer, indentation);
        else 
            throw new ArgumentException("no redirection or description");
    }
    public override IEnumerable<TResult> Query<TResult>(Func<TResult, bool>? predicate = null)
    {
        predicate ??= x => true;
        IEnumerable<TResult> result = [];
        if (this is TResult maybe && predicate(maybe))
            result = [maybe];
        if (Redirection != null)
            result = result.Concat(Redirection.Query(predicate));
        if (Description != null)
            result = result.Concat(Description.Query(predicate));
        return result;
    }
}