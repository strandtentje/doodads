using Ziewaar.RAD.Doodads.RKOP.Constructor;
using Ziewaar.RAD.Doodads.RKOP.Text;

namespace Ziewaar.RAD.Doodads.RKOP.Blocks;
#nullable enable
public class ServiceDescription<TResultSink> : ServiceExpression<TResultSink>
    where TResultSink : class, IInstanceWrapper, new()
{
    public SerializableConstructor Constructor = new();
    public SerializableBranchBlock<TResultSink> Children = new();
    public CursorText TextScope = CursorText.Empty;
    protected override bool ProtectedUpdateFrom(ref CursorText text)
    {
        TextScope = text;
        if (!Constructor.UpdateFrom(ref text))
            return false;
        ResultSink ??= new();
        Children.UpdateFrom(ref text);
        return true;
    }
    public override void HandleChanges()
    {        
        if (ResultSink == null)
            throw new ArgumentException("no result sink", nameof(ResultSink));
        if (Constructor.ServiceTypeName == null)
            throw new ArgumentException("no service type", nameof(Constructor.ServiceTypeName));
        ResultSink.SetDefinition(
            TextScope,
            Constructor.ServiceTypeName,
            Constructor.PrimaryExpression.GetValue(),
            Constructor.Constants.ToSortedList(),
            new SortedList<string, ServiceExpression<TResultSink>>((Children.Branches ?? []).ToDictionary(x => x.key, x => x.value)));
    }
    public override void Purge()
    {
        Children.Purge();
        ResultSink?.Cleanup();
    }
    public override void WriteTo(StreamWriter writer, int indentation = 0)
    {
        Constructor.WriteTo(writer, indentation);
        Children.WriteTo(writer, indentation);
    }
    public override IEnumerable<TResult> Query<TResult>(Func<TResult, bool>? predicate = null)
    {
        predicate ??= x => true;
        IEnumerable<TResult> results = [];
        if (this is TResult maybe && predicate(maybe))
            results = [maybe];
        if (Children.Branches != null)
            results = results.Concat(Children.Branches.SelectMany(x => x.value.Query(predicate)));
        return results;
    }
}