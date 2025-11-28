#nullable enable
using Ziewaar;
using Ziewaar.RAD.Doodads.RKOP.Text;

namespace Ziewaar.RAD.Doodads.RKOP.SeriesParsers;
public abstract class SerializableServiceSeries<TResultSink> :
    ServiceExpression<TResultSink>
    where TResultSink : class, IInstanceWrapper, new()
{
    public List<(bool isAlt, ServiceExpression<TResultSink> service)>? Children;
    protected abstract ServiceExpression<TResultSink> CreateChild();
    protected abstract TokenDescription PrimaryCouplerToken { get; }
    protected virtual TokenDescription? AltCouplerToken => null;
    protected abstract void SetChildren(
        TResultSink sink,
        (bool isAlt, ServiceExpression<TResultSink> service)[] children);
    protected override bool ProtectedUpdateFrom(ref CursorText text)
    {
        Children ??= new();
        ResultSink ??= new();
        if (Children.Any())
            throw new Exception("Can't give multiple branches the same name");
        Token couplerToken;
        Token? altCouplerToken;
        bool expectingMoreInSequence = true;
        ServiceExpression<TResultSink>? childWeJustRead = null;
        while (expectingMoreInSequence)
        {
            childWeJustRead = CreateChild();
            if (!childWeJustRead.UpdateFrom($"{CurrentNameInScope}_{Children.Count}", ref text))
                childWeJustRead = null;

            text = text.SkipWhitespace().TakeToken(PrimaryCouplerToken, out couplerToken);
            if (AltCouplerToken != null)
                text = text.TakeToken(AltCouplerToken, out altCouplerToken);
            else
                altCouplerToken = null;
            switch ((couplerToken.IsValid, altCouplerToken?.IsValid == true))
            {
                case (true, false):
                    if (childWeJustRead != null)
                    {
                        Children.Add((false, childWeJustRead));
                        childWeJustRead = null;
                    }
                    break;
                case (false, true):
                    if (childWeJustRead != null)
                    {
                        Children.Add((true, childWeJustRead));
                        childWeJustRead = null;
                    }
                    break;
                case (true, true):
                    throw new SyntaxException(
                        text,
                        $"expected `{PrimaryCouplerToken.HumanReadable}` OR `{AltCouplerToken?.HumanReadable ?? ""}` to couple services but got both");
                default:
                    expectingMoreInSequence = false;
                    break;
            }

        }
        if (childWeJustRead != null)
            Children.Add((false, childWeJustRead));
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
            serviceExpression.service.Purge();
        Children = null;
    }
    public override IEnumerable<TResult> Query<TResult>(Func<TResult, bool>? predicate = null)
    {
        predicate ??= x => true;
        IEnumerable<TResult> result = [];
        if (this is TResult maybe && predicate(maybe))
            result = [maybe];
        return result.Concat(Children.SelectMany(x => x.service.Query(predicate)));
    }
}