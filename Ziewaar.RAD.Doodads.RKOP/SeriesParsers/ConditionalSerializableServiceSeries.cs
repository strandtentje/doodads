#nullable enable
using Ziewaar;
using Ziewaar.RAD.Doodads.RKOP.Blocks;
using Ziewaar.RAD.Doodads.RKOP.Text;

namespace Ziewaar.RAD.Doodads.RKOP.SeriesParsers;
public class ConditionalSerializableServiceSeries<TResultSink> :
    SerializableServiceSeries<TResultSink>
    where TResultSink : class, IInstanceWrapper, new()
{
    protected override ServiceExpression<TResultSink> CreateChild() =>
        new ServiceDescription<TResultSink>();
    protected override TokenDescription PrimaryCouplerToken => TokenDescription.DefaultBranchCoupler;
    protected override TokenDescription? AltCouplerToken => TokenDescription.ObelusCoupler;
    protected override void SetChildren(
        TResultSink sink,
        (bool isAlt, ServiceExpression<TResultSink> service)[] children) =>
        sink.SetContinueSequence(children);
    public override void WriteTo(StreamWriter writer, int indentation = 0)
    {
        if (Children == null || Children.Count < 1)
            throw new ArgumentException("no children", nameof(Children));
        Children.ElementAt(0).service.WriteTo(writer, indentation);
        for (var i = 1; i < Children.Count; i++)
        {
            if (Children.ElementAt(i - 1).isAlt)
                writer.Write('÷');
            else
                writer.Write(':');
            Children.ElementAt(i).service.WriteTo(writer, indentation);
        }
    }
}