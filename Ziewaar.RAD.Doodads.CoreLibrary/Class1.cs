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

public class SinkingInteractionWith<TData>(IInteraction parent, ITagged origin, TData newSink)
    : ISinkingInteraction<TData>
{
    public IInteraction Parent => parent;
    public SortedList<string, object> Variables => parent.Variables;
    public ITaggedData<TData> TaggedData { get; } = new MimicTagged<TData>(origin, newSink);
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
            return new SinkingInteractionWith<StreamWriter>(interaction, candidateWriterOwner.TaggedData,
                candidateWriterOwner.TaggedData.Data);
        if (interaction.TryGetClosest<ISinkingInteraction<Stream>>(out var candidateStreamOwner))
            return new SinkingInteractionWith<StreamWriter>(interaction, candidateStreamOwner.TaggedData,
                new StreamWriter(candidateStreamOwner.TaggedData.Data));
        return new SinkingInteractionWith<StreamWriter>(interaction, NullTagged.Instance,
            new StreamWriter(NullStream.Instance));
    }

    public static ISinkingInteraction<Stream> ResurfaceStream(this IInteraction interaction)
    {
        if (interaction.TryGetClosest<ISinkingInteraction<Stream>>(out var candidateStreamOwner))
            return new SinkingInteractionWith<Stream>(interaction, candidateStreamOwner.TaggedData,
                candidateStreamOwner.TaggedData.Data);
        return new SinkingInteractionWith<Stream>(interaction, NullTagged.Instance, NullStream.Instance);
    }

    public static bool RequireUpdate<TData>(this ISinkingInteraction<TData> interaction, long newState)
    {
        var tag = interaction.TaggedData.Tag;
        var result = tag.State switch
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

public class NullTagged : ITagged
{
    public static NullTagged Instance { get; } = new NullTagged();
    
    public SidechannelTag Tag
    {
        get => SidechannelTag.InvariantNull;
        set { }
    }
}

public class ServiceConstants : SortedList<string, object>
{
    public TimeSpan LastChange;
}

public static class SortedListExtensions
{
    public static TResult InsertIgnore<TResult>(this SortedList<string, object> list, string key, TResult defaultValue = default)
    {
        if (!list.TryGetValue(key, out var candidateObjectValue))
            list.Add(key, candidateObjectValue = defaultValue);
        if (candidateObjectValue is not TResult candidateResultValue) 
            list[key] = candidateResultValue = defaultValue;
        return candidateResultValue;
    }
    
    public static SortedList<string, object> ToSortedList(this Exception ex, bool includeStack = true, bool includeData = true, bool includeInner = true)
    {
        var result = new SortedList<string, object>
        {
            ["Type"] = ex.GetType().FullName,
            ["Message"] = ex.Message,
            ["Source"] = ex.Source,
            ["HResult"] = ex.HResult,
            ["TargetSite"] = ex.TargetSite?.ToString()
        };

        if (includeStack && !string.IsNullOrEmpty(ex.StackTrace))
            result["StackTrace"] = ex.StackTrace;

        if (includeData && ex.Data?.Count > 0)
        {
            var dataDict = new Dictionary<string, object>();
            foreach (var key in ex.Data.Keys)
            {
                if (key is string strKey)
                    dataDict[strKey] = ex.Data[key];
                else
                    dataDict[key?.ToString() ?? "<null>"] = ex.Data[key];
            }
            result["Data"] = dataDict;
        }

        if (includeInner && ex.InnerException != null)
        {
            result["InnerException"] = ToSortedList(ex.InnerException, includeStack, includeData, includeInner);
        }

        return result;
    }
}


public class VariablesInteraction(IInteraction parent, SortedList<string, object> variables) : IInteraction
{
    public IInteraction Parent => parent;
    public SortedList<string, object> Variables => variables;
}


public interface IService
{
    void Enter(
        ServiceConstants serviceConstants,
        IInteraction interaction);
}