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
    protected override ParityParsingState ProtectedUpdateFrom(ref CursorText text)
    {
        var state = ParityParsingState.Void;        
        if (Children == null)
            Children = new();
        var wasNew = ResultSink == null;
        if (wasNew) ResultSink = new();
        Token couplerToken;
        int currentChildNumber = 0;
        do
        {
            ServiceExpression<TResultSink> prelimChild;
            if (Children.Count > currentChildNumber)
            {
                prelimChild = Children.ElementAt(currentChildNumber);
                Children.RemoveAt(currentChildNumber);
            } else
            {
                prelimChild = CreateChild();
            }
            var childState = prelimChild.UpdateFrom($"{CurrentNameInScope}_{Children.Count}", ref text);
            if (childState > ParityParsingState.Void)
            {
                Children.Insert(currentChildNumber, prelimChild);
                state |= childState;
            }
            text = text.SkipWhile(char.IsWhiteSpace).TakeToken(CouplerToken, out couplerToken);
            currentChildNumber++;
        } while (couplerToken.IsValid);
        if (wasNew)
            return ParityParsingState.New;
        else 
            return state;
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