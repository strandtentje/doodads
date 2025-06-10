#nullable enable
namespace Ziewaar.RAD.Doodads.RKOP;

public static class ServiceExpressionExtensions
{
    public static IEnumerable<TResultOut> QueryAllServices<TResultIn, TResultOut, TInner>(
        this IEnumerable<TResultIn>? collection, 
        Func<TResultOut, bool>? predicate = null)
        where TResultIn : ServiceExpression<TInner>
        where TResultOut : ServiceExpression<TInner>
        where TInner : class, IInstanceWrapper, new()
    {
        predicate ??= x => true;
        return collection.SelectMany(x => x.Query(predicate));
    }
}
