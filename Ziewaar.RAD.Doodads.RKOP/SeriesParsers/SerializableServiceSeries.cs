#nullable enable
using Ziewaar;
using Ziewaar.RAD.Doodads.RKOP.Text;

namespace Ziewaar.RAD.Doodads.RKOP.SeriesParsers;
public abstract class SerializableServiceSeries<TResultSink> :
    ServiceExpression<TResultSink>
    where TResultSink : class, IInstanceWrapper, new()
{
    public List<ServiceExpression<TResultSink>>? Children;
    protected abstract ServiceExpression<TResultSink> CreateChild();
    protected abstract TokenDescription CouplerToken { get; }
    protected abstract void SetChildren(TResultSink sink, ServiceExpression<TResultSink>[] children);
    protected override bool ProtectedUpdateFrom(ref CursorText text)
    {
        Children ??= new();
        ResultSink ??= new();
        if (Children.Any())
            throw new Exception("oh no.");
        Token couplerToken;
        do
        {
            var prelimChild = CreateChild();
            if (prelimChild.UpdateFrom($"{CurrentNameInScope}_{Children.Count}", ref text))
                Children.Add(prelimChild);
            text = text.SkipWhile(char.IsWhiteSpace).TakeToken(CouplerToken, out couplerToken);
        } while (couplerToken.IsValid);
        return Children.Count > 0;
    }
    public override void HandleChanges()
    {
        if (ResultSink == null)
            throw new ArgumentException("no result sink", nameof(ResultSink));        
        SetChildren(ResultSink, Children?.ToArray() ?? []);
    }
    public override void Purge()
    {
        if (Children == null) return;
        foreach (var serviceExpression in Children)
            serviceExpression.Purge();
        Children = null;
    }
    public override IEnumerable<TResult> Query<TResult>(Func<TResult, bool>? predicate = null)
    {
        predicate ??= x => true;
        IEnumerable<TResult> result = [];
        if (this is TResult maybe && predicate(maybe))
            result = [maybe];
        return result.Concat(Children.SelectMany(x => x.Query(predicate)));
    }
}