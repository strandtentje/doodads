namespace Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;
#nullable enable
public static class InteractionExtensions
{
    public static bool TryGetClosest<TInteraction>(
        this IInteraction childInteraction,        
        out TInteraction? candidateParentInteraction,
        Func<TInteraction, bool>? predicate = null)
        where TInteraction : IInteraction
    {
        switch (childInteraction)
        {
            case StopperInteraction _:
                candidateParentInteraction = default;
                return false;
            case TInteraction alreadySuitableInteraction
            when predicate == null || predicate(alreadySuitableInteraction):
                candidateParentInteraction = alreadySuitableInteraction;
                return true;
            default:
                return childInteraction.Stack.TryGetClosest(out candidateParentInteraction, predicate);
        }
    }
#nullable enable
    public static bool TryFindVariable<TType>(
        this IInteraction interaction,
        string key,
        out TType? candidateValue)
    {
        IReadOnlyDictionary<string, object>? previousVariables = null;
        for(;interaction != null; interaction = interaction.Stack)
        {
            if (interaction is StopperInteraction)
                break;
            if (previousVariables != interaction.Memory && 
                interaction.Memory.TryGetValue(key, out object value) && 
                value is TType foundResult)
            {
                candidateValue = foundResult;
                return true;
            }
            previousVariables = interaction.Memory;
        }
        candidateValue = default;
        return false;
    }
}
