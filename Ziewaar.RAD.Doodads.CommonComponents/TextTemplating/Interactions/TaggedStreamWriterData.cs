namespace Ziewaar.RAD.Doodads.CommonComponents;

public class TaggedStreamWriterData(StreamWriter data, SidechannelState state) : ITaggedData<StreamWriter>
{
    public SidechannelTag Tag { get; set; } = new() { IsTainted = false, Stamp = 0, State = state };
    public StreamWriter Data => data;
}