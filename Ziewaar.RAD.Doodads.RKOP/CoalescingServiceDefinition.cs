#nullable enable
namespace Ziewaar.RAD.Doodads.RKOP;
public class CoalescingServiceDefinition<TResultSink> :
    ServiceExpression<TResultSink>
    where TResultSink : class, IInstanceWrapper, new()
{
    private SerializableRedirection<TResultSink>? Redirection;
    private ServiceDescription<TResultSink>? Description;
    protected override ParityParsingState ProtectedUpdateFrom(ref CursorText text)
    {
        if (Redirection == null)
            Redirection = new();
        var state = Redirection.UpdateFrom($"r_{this.CurrentNameInScope}", ref text);
        if (state == ParityParsingState.Void)
            Redirection = null;
        else
            return state;

        if (Description == null)
            Description = new();
        return Description.UpdateFrom($"d_{this.CurrentNameInScope}", ref text);
    }
    public override void HandleChanges()
    {
        if (ResultSink == null)
            throw new ArgumentException("no result sink", nameof(ResultSink));
        else if (Redirection != null)
            ResultSink.SetSoftLink(this.Redirection);
        else if (Description != null)
            ResultSink.SetHardLink(this.Description);
        else
            throw new ArgumentException("no redirection or description");
    }
    public override void Purge()
    {
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
    public override TDesiredResultSink? GetSingleOrDefault<TDesiredResultSink>(
        Func<TDesiredResultSink, bool>? predicate = null) where TDesiredResultSink : class
    {
        predicate ??= x => true;
        if (this is TDesiredResultSink selfDesired && predicate(selfDesired))
            return selfDesired;
        if (Redirection is TDesiredResultSink desirableRedirection && predicate(desirableRedirection))
            return desirableRedirection;
        else if (Description is TDesiredResultSink desirableDescription && predicate(desirableDescription))
            return desirableDescription;
        else
            return null;
    }
}