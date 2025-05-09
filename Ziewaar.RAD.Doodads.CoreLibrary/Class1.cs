namespace Ziewaar.RAD.Doodads.CoreLibrary;

public enum SidechannelState
{
    /// <summary>
    /// Means the accompanying object should always be updated
    /// </summary>
    Always,
    /// <summary>
    /// Means the accompanying object will null-sink whatever happens.
    /// </summary>
    Never,
    /// <summary>
    /// Means update when stamp is different
    /// </summary>
    StampDifferent,
    /// <summary>
    /// Update when stamp is lower
    /// </summary>
    StampLower,
    /// <summary>
    /// Update when stamp is greater
    /// </summary>
    StampGreater,
}

public class SidechannelTag
{
    public static readonly SidechannelTag InvariantNull = new SidechannelTag()
    {
        Stamp = 0, State = SidechannelState.Never,
    };
    public static readonly SidechannelTag Always = new SidechannelTag()
    {
        Stamp = 0, State = SidechannelState.Always,
    };
    public static readonly SidechannelTag UpdateWhenChanged = new()
    {
        Stamp = 0, State = SidechannelState.StampDifferent,
    };
    public SidechannelState State;
    public bool IsTainted;
    public long Stamp;
}

public interface ITagged
{
    SidechannelTag Tag { get; set; }
}

public interface ITaggedData<TDataType> : ITagged
{
    TDataType Data { get; }
}

public interface ISourcingInteraction<TDataType> : IInteraction
{
    ITaggedData<TDataType> TaggedData { get; }
}

public interface ISinkingInteraction<TDataType> : IInteraction
{
    ITaggedData<TDataType> TaggedData { get; }
}

public class SinkingInteractionWithWriter(IInteraction parent, ITagged origin, StreamWriter newSink)
    : ISinkingInteraction<StreamWriter>
{
    public IInteraction Parent => parent;
    public SortedList<string, object> Variables => parent.Variables;
    public ITaggedData<StreamWriter> TaggedData { get; } = new MimicTagged<StreamWriter>(origin, newSink);
}

public class MimicTagged<TMimic>(ITagged original, TMimic mimicObject) : ITaggedData<TMimic>
{
    public SidechannelTag Tag
    {
        get => original.Tag;
        set => original.Tag = value;
    }

    public TMimic Data => mimicObject;
}

[AttributeUsage(AttributeTargets.Event)]
public class WildcardBranchAttribute() : Attribute;

[AttributeUsage(AttributeTargets.Event)]
public class NamedBranchAttribute() : Attribute;

[AttributeUsage(AttributeTargets.Property)]
public class SuggestedWildcardsAttribute() : Attribute;

public interface IInteraction
{
    IInteraction Parent { get; }
    SortedList<string, object> Variables { get; }
}

public interface IWildcardTargetInteraction : IInteraction
{
    string Pattern { get; }
}

public static class InteractionExtensions
{
    public static bool TryGetClosest<TInteraction>(
        this IInteraction childInteraction,
        out TInteraction candidateParentInteraction)
        where TInteraction : IInteraction
    {
        switch (childInteraction)
        {
            case null:
                candidateParentInteraction = default;
                return false;
            case TInteraction alreadySuitableInteraction:
                candidateParentInteraction = alreadySuitableInteraction;
                return true;
            default:
                return childInteraction.Parent.TryGetClosest<TInteraction>(out candidateParentInteraction);
        }
    }

    public static ISinkingInteraction<StreamWriter> ResurfaceWriter(this IInteraction interaction)
    {
        if (interaction.TryGetClosest<ISinkingInteraction<StreamWriter>>(out var candidateWriterOwner))
            return new SinkingInteractionWithWriter(interaction, candidateWriterOwner.TaggedData,
                candidateWriterOwner.TaggedData.Data);
        if (interaction.TryGetClosest<ISinkingInteraction<Stream>>(out var candidateStreamOwner))
            return new SinkingInteractionWithWriter(interaction, candidateStreamOwner.TaggedData,
                new StreamWriter(candidateStreamOwner.TaggedData.Data));
        return new SinkingInteractionWithWriter(interaction, NullTagged.Instance,
            new StreamWriter(NullStream.Instance));
    }
}

public class NullTagged : ITagged
{
    public static NullTagged Instance { get; } = new NullTagged();
    
    public SidechannelTag Tag
    {
        get => SidechannelTag.InvariantNull;
        set { }
    }
}

public interface IService
{
    void Enter(
        SortedList<string, object> constants,
        IInteraction interaction);
}