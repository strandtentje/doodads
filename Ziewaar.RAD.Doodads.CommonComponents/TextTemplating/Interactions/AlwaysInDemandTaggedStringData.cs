namespace Ziewaar.RAD.Doodads.CommonComponents;

public class AlwaysInDemandTaggedStringData(StreamWriter data) : ITaggedData<StreamWriter>
{
    public SidechannelTag Tag
    {
        get => SidechannelTag.Always;
        set { }
    }

    public StreamWriter Data => data;
}