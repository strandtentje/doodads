#nullable enable
using Ziewaar.RAD.Doodads.RKOP.Text;

namespace Ziewaar.RAD.Doodads.RKOP;
public abstract class ServiceExpression<TResultSink> where TResultSink : class, IInstanceWrapper, new()
{
    public string? CurrentNameInScope { get; private set; }
    public TResultSink? ResultSink { get; protected set; }
    public ParityParsingState UpdateFrom(string givenScopeName, ref CursorText text, bool forceReload = false)
    {
        this.CurrentNameInScope = givenScopeName;
        var state = ProtectedUpdateFrom(ref text);
        switch (state)
        {
            case ParityParsingState.Void:
                Purge();
                ResultSink = null;                
                break;
            case ParityParsingState.Unchanged:
                if (forceReload) HandleChanges();
                break;
            default:
                text[this.CurrentNameInScope] = this;
                HandleChanges();
                break;
        }
        return state;
    }
    protected abstract ParityParsingState ProtectedUpdateFrom(ref CursorText text);
    public abstract void HandleChanges();
    public abstract void Purge();
    public abstract void WriteTo(StreamWriter writer, int indentation = 0);
    public abstract IEnumerable<TResult> Query<TResult>(
        Func<TResult, bool>? predicate = null)
        where TResult : ServiceExpression<TResultSink>;
}