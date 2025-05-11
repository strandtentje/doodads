namespace Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;

public interface ISourcingInteraction<TDataType> : IInteraction
{
    ITaggedData<TDataType> TaggedData { get; }
}
