using System.Runtime.CompilerServices;
using System.Threading;
using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;

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

    public static bool TryGetCustom<TType>(
        this IInteraction childInteraction,
        out TType? customPayload)
    {
        if (!TryGetClosest<CustomInteraction<TType>>(childInteraction, out var x)
            || x == null)
        {
            customPayload = default;
            return false;
        }
        else
        {
            customPayload = x.Payload;
            return true;
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

    public static IEnumerable<TInteraction> GetAllOf<TInteraction>(
        this IInteraction offset,
        Func<TInteraction, bool>? predicate = null) where TInteraction : IInteraction
    {
        switch (offset)
        {
            case StopperInteraction _:
                return [];
            case TInteraction suitable
                when predicate == null || predicate(suitable):
                return offset.Stack.GetAllOf(predicate).Concat([suitable]);
            default:
                return offset.Stack.GetAllOf(predicate);
        }
    }

    public static void RunCancellable(this (IInteraction interaction, string? name) offset,
        Action<RepeatInteraction> run)
    {
        using CancellationTokenSource cts = new();
        var cancellers = offset.interaction.GetAllOf<CancellationInteraction>(
            x => string.IsNullOrWhiteSpace(x.Name) || x.Name == offset.name);
        void Cancelled(object? o, EventArgs e) => cts.Cancel();
        foreach (var canceller in cancellers)
            canceller.Cancelled += Cancelled;
        var ri = new RepeatInteraction(offset.name ?? Guid.NewGuid().ToString(), offset.interaction, cts.Token);
        try
        {
            run(ri);
        }
        finally
        {
            foreach (var canceller in cancellers)
                canceller.Cancelled -= Cancelled;
        }
    }

    public static bool IsCancelled(this IInteraction interaction) => ((RepeatInteraction)interaction).CancellationToken.IsCancellationRequested;
    public static CancellationToken GetCancellationToken(this IInteraction interaction) => ((RepeatInteraction)interaction).CancellationToken;
#nullable enable
    public static bool TryFindVariable<TType>(
        this IInteraction interaction,
        string key,
        out TType? candidateValue)
    {
        IReadOnlyDictionary<string, object>? previousVariables = null;
        for (; interaction != null; interaction = interaction.Stack)
        {
            if (interaction.Memory == EmptyReadOnlyDictionary.Instance && interaction is StopperInteraction)
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

    public static IInteraction AppendCustom<TCustom>(
        this IInteraction interaction,
        TCustom customValue) => new CustomInteraction<TCustom>(interaction, customValue);

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

    public static void SinkText(
        this (IService service, IInteraction interaction) offset,
        CallForInteraction? invoke,
        out string sunkText)
    {
        TextSinkingInteraction sinker = new TextSinkingInteraction(offset.interaction);
        invoke?.Invoke(offset.service, sinker);
        sunkText = sinker.ReadAllText();
    }
    public static void SinkEnum<TEnum>(
        this (IService service, IInteraction interaction) offset,
        CallForInteraction? invoke,
        TEnum fallbackValue,
        out TEnum result) where TEnum : struct, IConvertible
    {
        TextSinkingInteraction sinker = new TextSinkingInteraction(offset.interaction);
        invoke?.Invoke(offset.service, sinker);
        var sunkText = sinker.ReadAllText();
        if (!Enum.TryParse<TEnum>(sunkText, out result))
            result = fallbackValue;
    }

}