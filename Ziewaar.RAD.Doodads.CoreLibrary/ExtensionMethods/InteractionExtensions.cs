namespace Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;
#nullable enable
public class ResurfacedSinkingInteraction(
    IInteraction canonicalInteraction,
    ISinkingInteraction sinkingInteraction)
    : ISinkingInteraction
{
    public IInteraction Stack => canonicalInteraction;
    public object Register => canonicalInteraction.Register;
    public IReadOnlyDictionary<string, object> Memory => canonicalInteraction.Memory;
    public Encoding TextEncoding => sinkingInteraction.TextEncoding;
    public Stream SinkBuffer => sinkingInteraction.SinkBuffer;
    public string[] SinkContentTypePattern => sinkingInteraction.SinkContentTypePattern;
    public string? SinkTrueContentType
    {
        get => sinkingInteraction.SinkTrueContentType;
        set => sinkingInteraction.SinkTrueContentType = value;
    }
    public long LastSinkChangeTimestamp
    {
        get => sinkingInteraction.LastSinkChangeTimestamp;
        set => sinkingInteraction.LastSinkChangeTimestamp = value;
    }
    public string Delimiter => sinkingInteraction.Delimiter;
    public void SetContentLength64(long contentLength) => sinkingInteraction.SetContentLength64(contentLength);
}
public static class InteractionExtensions
{
    public static IInteraction ResurfaceToSink(
        this IInteraction canonicalInteraction, 
        ISinkingInteraction? sinkingInteraction)
    {
        if (sinkingInteraction == null)
            return canonicalInteraction;
        else 
            return new ResurfacedSinkingInteraction(canonicalInteraction, sinkingInteraction);
    }
    public static IEnumerable<TInteraction> FindInStack<TInteraction>(
        this IInteraction interaction)
        where TInteraction : IInteraction
    {
        while(true)
        {
            if (interaction is StopperInteraction) yield break;
            else if (interaction is TInteraction tinteraction) yield return tinteraction;
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
}