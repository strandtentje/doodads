namespace Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

public class TaggedStreamWriterData(StreamWriter data, SidechannelState state) : ITaggedData<StreamWriter>
{
    public SidechannelTag Tag { get; set; } = new() { IsTainted = false, Stamp = 0, TaintCondition = state };
    public StreamWriter Data => data;
}