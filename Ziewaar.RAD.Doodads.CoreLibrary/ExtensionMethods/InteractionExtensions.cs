using System.Linq;

namespace Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;

public static class InteractionExtensions
{
    public static bool TryGetClosest<TInteraction>(
        this IInteraction childInteraction,
        out TInteraction candidateParentInteraction,
        Func<TInteraction, bool> predicate = null)
        where TInteraction : IInteraction
    {
        switch (childInteraction)
        {
            case null:
                candidateParentInteraction = default;
                return false;
            case TInteraction alreadySuitableInteraction
            when predicate == null || predicate(alreadySuitableInteraction):
                candidateParentInteraction = alreadySuitableInteraction;
                return true;
            default:
                return childInteraction.Parent.TryGetClosest(out candidateParentInteraction, predicate);
        }
    }
#nullable enable
    public static ISinkingInteraction<StreamWriter>? ResurfaceWriter(
        this IInteraction interaction) =>
        interaction.TryGetClosest<ISinkingInteraction<StreamWriter>>(out var candidateWriterOwner) ? 
        candidateWriterOwner : null;
    public static ISinkingInteraction<Stream>? ResurfaceStream(
        this IInteraction interaction) => 
        interaction.TryGetClosest<ISinkingInteraction<Stream>>(out var candidateStreamOwner) ? 
        candidateStreamOwner : null;    
    public static bool TryRequireStreamingUpdate(
        this IInteraction interaction,
        long stamp,
        out IInteraction? source,
        out StreamWriter? writer,
        out string? delimiter)
    {
        if (interaction.ResurfaceWriter() is ISinkingInteraction<StreamWriter> writerInteraction)
        {
            source = writerInteraction;
            writer = writerInteraction.TaggedData.Data;
            delimiter = writerInteraction.Delimiter;
            return writerInteraction.RequireUpdate(stamp);
        }
        else if (interaction.ResurfaceWriter() is ISinkingInteraction<Stream> streamInteraction)
        {
            source = streamInteraction;
            writer = new StreamWriter(streamInteraction.TaggedData.Data);
            delimiter = streamInteraction.Delimiter;
            return streamInteraction.RequireUpdate(stamp);
        }
        else
        {
            source = null;
            writer = null;
            delimiter = null;
            return false;
        }
    }
    public static bool IsContentTypeApplicable(
        this IInteraction interaction,
        string contentType) => 
        interaction is not IContentTypeSink contentTypeSink || 
        contentTypeSink.Accept.Any(x => ContentTypeMatcher.IsMatch(x, contentType));
#nullable disable
    public static bool RequireUpdate<TData>(this ISinkingInteraction<TData> interaction, long newState)
    {
        var tag = interaction.TaggedData.Tag;
        var result = tag.TaintCondition switch
        {
            SidechannelState.Always => true,
            SidechannelState.StampDifferent when tag.Stamp != newState => true,
            SidechannelState.StampGreater when newState > tag.Stamp => true,
            SidechannelState.StampLower when newState < tag.Stamp => true,
            _ => false
        };
        if (result)
        {
            tag.Stamp = newState;
            tag.IsTainted = true;
        }
        return result;
    }
}
