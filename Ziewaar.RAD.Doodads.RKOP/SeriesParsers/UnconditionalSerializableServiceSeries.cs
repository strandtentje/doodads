#nullable enable
using Ziewaar;
using Ziewaar.RAD.Doodads.RKOP.Text;

namespace Ziewaar.RAD.Doodads.RKOP.SeriesParsers;
public class UnconditionalSerializableServiceSeries<TResultSink> :
    SerializableServiceSeries<TResultSink>
    where TResultSink : class, IInstanceWrapper, new()
{
    protected override ServiceExpression<TResultSink> CreateChild() =>
        new AlternativeSerializableServiceSeries<TResultSink>();
    protected override TokenDescription PrimaryCouplerToken => TokenDescription.AmpersandP;
    protected override void SetChildren(TResultSink sink, (bool isAlt, ServiceExpression<TResultSink> service)[] children) =>
        sink.SetUnconditionalSequence(children.Select(x => x.service).ToArray());
    public override void WriteTo(StreamWriter writer, int indentation = 0)
    {
        if (Children == null || Children.Count < 1)
            throw new ArgumentException("no children", nameof(Children));
        if (CurrentNameInScope == null)
            throw new ArgumentNullException("no name", nameof(CurrentNameInScope));
        Children.ElementAt(0).service.WriteTo(writer, indentation);
        var nameIndentation = CurrentNameInScope.Length;
        for (var i = 1; i < Children.Count; i++)
        {
            var child = Children.ElementAt(i);
            writer.WriteLine();
            writer.Write(new string(' ', indentation + nameIndentation));
            writer.Write("& ");
            child.service.WriteTo(writer, indentation + nameIndentation);
        }
    }
}