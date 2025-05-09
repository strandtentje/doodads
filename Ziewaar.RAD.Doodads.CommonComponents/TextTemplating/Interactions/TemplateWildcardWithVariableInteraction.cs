namespace Ziewaar.RAD.Doodads.CommonComponents;

public class TemplateWildcardWithVariableInteraction(IInteraction parent, string pattern) : IWildcardTargetInteraction
{
    public SortedList<string, object> Variables { get; } = new() { { "template_segment_name", pattern } };
    public IInteraction Parent { get; } = parent;
    public string Pattern { get; } = pattern;
}