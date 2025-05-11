namespace Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;

public interface ITaggedData<TDataType> : ITagged
{
    TDataType Data { get; }
}
