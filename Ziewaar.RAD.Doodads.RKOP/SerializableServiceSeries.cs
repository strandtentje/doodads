#nullable enable
namespace Ziewaar.RAD.Doodads.RKOP;
public abstract class SerializableServiceSeries<TResultSink> :
    ServiceExpression<TResultSink>
    where TResultSink : class, IInstanceWrapper, new()
{
    public Stack<ServiceExpression<TResultSink>>? Children;
    protected abstract ServiceExpression<TResultSink> CreateChild();
    protected abstract TokenDescription CouplerToken { get; }
    protected abstract void SetChildren(TResultSink sink, ServiceExpression<TResultSink>[] children);
    protected override ParityParsingState ProtectedUpdateFrom(ref CursorText text)
    {
        var state = ParityParsingState.Void;
        if (Children == null)
            Children = new();
        Token couplerToken;
        do
        {
            var preliminaryChild =
                Children.Count > 0 ? Children.Pop() : CreateChild();
            var childState = preliminaryChild.UpdateFrom($"{CurrentNameInScope}_{Children.Count}", ref text);
            if (childState > ParityParsingState.Void)
            {
                Children.Push(preliminaryChild);
                state |= childState;
            }
            text = text.SkipWhile(char.IsWhiteSpace).TakeToken(CouplerToken, out couplerToken);
        } while (couplerToken.IsValid);

        return state;
    }
    public override void HandleChanges()
    {
        if (ResultSink == null)
            throw new ArgumentException("no result sink", nameof(ResultSink));
        if (Children == null)
            throw new ArgumentException("no children", nameof(Children));   
        SetChildren(ResultSink, Children.ToArray());
    }
    public override void Purge()
    {
        if (Children == null) return;
        foreach (var serviceExpression in Children)
            serviceExpression.Purge();
        Children = null;
    }
    public override TDesiredResultSink? GetSingleOrDefault<TDesiredResultSink>(
        Func<TDesiredResultSink, bool>? predicate = null) where TDesiredResultSink : class
    {
        predicate ??= x => true;
        if (this is TDesiredResultSink desiredResultSink && predicate(desiredResultSink))
            return desiredResultSink;
        
        var localMatches = Children?.OfType<TDesiredResultSink>().ToArray();
        return 
            localMatches?.Any() == true 
                ? localMatches.SingleOrDefault(predicate)
                : Children?.Select(x => x.GetSingleOrDefault(predicate)).SingleOrDefault();
    }
}