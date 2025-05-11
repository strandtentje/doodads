namespace Ziewaar.RAD.Doodads.CommonComponents;

public class TemplatePlaceholderRequestInteraction(IInteraction parent, string pattern)
    : RawStringSinkingInteraction(parent), IWildcardTargetInteraction
{
    public string Pattern => pattern;
}