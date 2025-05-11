namespace Ziewaar.RAD.Doodads.CommonComponents;

public class NamedTemplatePlaceholderRequestInteraction(IInteraction parent, string pattern)
    : TemplatePlaceholderRequestInteraction(parent, pattern)
{
    public override IReadOnlyDictionary<string, object> Variables { get; } = 
        new SortedList<string, object>() { { "template_segment_name", pattern } };
}