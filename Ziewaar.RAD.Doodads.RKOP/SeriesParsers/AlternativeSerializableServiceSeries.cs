#nullable enable
using Ziewaar;
using Ziewaar.RAD.Doodads.RKOP.Text;

namespace Ziewaar.RAD.Doodads.RKOP.SeriesParsers;
public class AlternativeSerializableServiceSeries<TResultSink> :
    SerializableServiceSeries<TResultSink>
    where TResultSink : class, IInstanceWrapper, new()
{
    public override ServiceExpression<TResultSink> CreateChild() =>
        new ConditionalSerializableServiceSeries<TResultSink>();
    protected override TokenDescription CouplerToken => TokenDescription.Pipe;
    protected override void SetChildren(TResultSink sink, ServiceExpression<TResultSink>[] children) => 
        sink.SetAlternativeSequence(children);
    public override void WriteTo(StreamWriter writer, int indentation = 0)
    {
        if (Children == null || Children.Count < 1)
            throw new ArgumentException("no children", nameof(Children));
        if (CurrentNameInScope == null)
            throw new ArgumentException("no name", nameof(CurrentNameInScope));
        Children.ElementAt(0).WriteTo(writer, indentation);
        var nameIndentation = CurrentNameInScope.Length;
        for (var i = 1; i < Children.Count; i++)
        {
            var child = Children.ElementAt(i);
            writer.WriteLine();
            writer.Write(new string(' ', indentation + nameIndentation));
            writer.Write("| ");
            child.WriteTo(writer, indentation + nameIndentation + 2);
        }
    }
}