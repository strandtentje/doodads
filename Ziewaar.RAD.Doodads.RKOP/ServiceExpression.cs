#nullable enable
using Ziewaar.RAD.Doodads.RKOP.Text;

namespace Ziewaar.RAD.Doodads.RKOP;
public abstract class ServiceExpression<TResultSink> where TResultSink : class, IInstanceWrapper, new()
{
    public string? CurrentNameInScope { get; private set; }
    public TResultSink? ResultSink { get; protected set; }
    public bool UpdateFrom(string givenScopeName, ref CursorText text, bool forceReload = false)
    {
        this.CurrentNameInScope = givenScopeName;
        if (ProtectedUpdateFrom(ref text))
        {
            text[CurrentNameInScope] = this;
            HandleChanges();
            return true;
        }
        else
        {
            Purge();
            return false;
        }
    }
    protected abstract bool ProtectedUpdateFrom(ref CursorText text);
    public abstract void HandleChanges();
    public abstract void Purge();
    public abstract void WriteTo(StreamWriter writer, int indentation = 0);
    public abstract IEnumerable<TResult> Query<TResult>(
        Func<TResult, bool>? predicate = null)
        where TResult : ServiceExpression<TResultSink>;
}