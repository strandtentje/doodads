namespace Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;
#nullable enable

public static class InteractionExtensions
{
    public static IInteraction ResurfaceToSink(
        this IInteraction canonicalInteraction,
        ISinkingInteraction? sinkingInteraction)
    {
        if (sinkingInteraction == null)
            return canonicalInteraction;
        else
            return new ResurfacedSinkingInteraction(canonicalInteraction,
                sinkingInteraction);
    }

    public static IEnumerable<TInteraction> FindInStack<TInteraction>(
        this IInteraction interaction)
        where TInteraction : IInteraction
    {
        while (true)
        {
            if (interaction is StopperInteraction) yield break;
            else if (interaction is TInteraction tinteraction)
                yield return tinteraction;
            interaction = interaction.Stack;
        }
    }

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
                return childInteraction.Stack.TryGetClosest(
                    out candidateParentInteraction, predicate);
        }
    }
#nullable enable
    public static bool TryFindVariable<TType>(
        this IInteraction interaction,
        string key,
        out TType? candidateValue)
    {
        IReadOnlyDictionary<string, object>? previousVariables = null;
        for (; interaction != null; interaction = interaction.Stack)
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

    public static IInteraction AppendRegister(
        this IInteraction interaction,
        object registerValue) =>
        new CommonInteraction(interaction, register: registerValue);

    public static IInteraction AppendMemory(
        this IInteraction interaction,
        params (string key, object value)[] members) =>
        interaction.AppendMemory(members.ToDictionary(x => x.key, x => x.value));

    public static IInteraction AppendMemory(
        this IInteraction interaction,
        IReadOnlyDictionary<string, object> dict) => new CommonInteraction(
        interaction, memory: dict, register: null);
}