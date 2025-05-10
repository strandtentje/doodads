namespace Ziewaar.RAD.Doodads.CommonComponents;

public class RawStringAlwaysWithVariableSinkingWildcardInteraction(IInteraction parent, string pattern)
    : RawStringAlwaysSinkingWildcardInteraction(parent, pattern)
{
    public override SortedList<string, object> Variables { get; } = new() { { "template_segment_name", pattern } };
}