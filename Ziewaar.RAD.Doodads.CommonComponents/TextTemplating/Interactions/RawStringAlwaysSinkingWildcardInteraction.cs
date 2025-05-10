namespace Ziewaar.RAD.Doodads.CommonComponents;

public class RawStringAlwaysSinkingWildcardInteraction(IInteraction parent, string pattern)
    : RawStringAlwaysSinkingInteraction(parent), IWildcardTargetInteraction
{
    public string Pattern => pattern;
}