namespace Ziewaar.RAD.Doodads.CommonComponents;

public class LatestTemplateUpdate() : ITaggedData<Stream>
{
    public SidechannelTag Tag { get; set; } = SidechannelTag.UpdateWhenChanged;
    public Stream Data { get; } = new MemoryStream();
}
