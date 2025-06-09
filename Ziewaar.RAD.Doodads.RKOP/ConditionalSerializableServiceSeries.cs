#nullable enable
namespace Ziewaar.RAD.Doodads.RKOP;
public class ConditionalSerializableServiceSeries<TResultSink> :
    SerializableServiceSeries<TResultSink> 
    where TResultSink : class, IInstanceWrapper, new()
{
    protected override ServiceExpression<TResultSink> CreateChild() =>
        new CoalescingServiceDefinition<TResultSink>();
    protected override TokenDescription CouplerToken => TokenDescription.DefaultBranchCoupler;
    protected override void SetChildren(TResultSink sink, ServiceExpression<TResultSink>[] children) =>
        sink.SetContinueSequence(children);
    public override void WriteTo(StreamWriter writer, int indentation = 0)
    {
        if (Children == null || Children.Count < 1)
            throw new ArgumentException("no children", nameof(Children));
        Children.ElementAt(0).WriteTo(writer, indentation);
        var nameIndentation = CurrentNameInScope.Length;
        for (var i = 1; i < Children.Count; i++)
        {
            writer.Write(':');
            Children.ElementAt(i).WriteTo(writer, indentation + nameIndentation + 2);
        }
    }
}