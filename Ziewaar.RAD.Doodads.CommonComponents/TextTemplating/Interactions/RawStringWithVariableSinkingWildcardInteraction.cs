namespace Ziewaar.RAD.Doodads.CommonComponents;

public class RawStringWithVariableSinkingWildcardInteraction(IInteraction parent, string pattern)
    : RawStringSinkingWildcardInteraction(parent, pattern)
{
    public override SortedList<string, object> Variables { get; } = new() { { "template_segment_name", pattern } };
}