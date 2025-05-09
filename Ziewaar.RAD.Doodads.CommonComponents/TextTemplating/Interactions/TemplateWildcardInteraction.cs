namespace Ziewaar.RAD.Doodads.CommonComponents;

public class TemplateWildcardInteraction(IInteraction parent, string pattern) : IWildcardTargetInteraction
{
    public IInteraction Parent => parent;
    public SortedList<string, object> Variables => parent.Variables;
    public string Pattern => pattern;
}