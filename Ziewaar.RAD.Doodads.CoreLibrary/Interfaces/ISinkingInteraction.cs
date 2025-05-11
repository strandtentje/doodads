namespace Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;

public interface ISinkingInteraction<TDataType> : IInteraction
{
    ITaggedData<TDataType> TaggedData { get; }
    string Delimiter { get; }
}
