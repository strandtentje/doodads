namespace Ziewaar.RAD.Doodads.RKOP;
#nullable enable
public class ServiceDescription<TResultSink> : ServiceExpression<TResultSink>
    where TResultSink : class, IInstanceWrapper, new()
{
    public SerializableConstructor Constructor = new();
    public SerializableBranchBlock<TResultSink> Children = new();
    public CursorText TextScope = CursorText.Empty;
    protected override ParityParsingState ProtectedUpdateFrom(ref CursorText text)
    {
        var state = ResultSink == null ? ParityParsingState.New : ParityParsingState.Unchanged;
        if (state == ParityParsingState.New)
            ResultSink = new TResultSink();
        TextScope = text;
        return state | Constructor.UpdateFrom(ref text) | Children.UpdateFrom(ref text);
    }
    public override void HandleChanges()
    {
        if (ResultSink == null)
            throw new ArgumentException("no result sink", nameof(ResultSink));
        if (Constructor.ServiceTypeName == null)
            throw new ArgumentException("no service type", nameof(Constructor.ServiceTypeName));
        if (Children.Branches == null)
            throw new ArgumentException("no branches", nameof(Children.Branches));
        ResultSink.SetDefinition(
            TextScope,
            Constructor.ServiceTypeName,
            Constructor.Constants.ToSortedList(),
            Children.Branches);
    }
    public override void Purge()
    {
        ResultSink?.Cleanup();
    }
    public override void WriteTo(StreamWriter writer, int indentation = 0)
    {
        Constructor.WriteTo(writer, indentation);
        Children.WriteTo(writer, indentation);
    }
}