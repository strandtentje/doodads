namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Interactions.Payloads;

public class ResponseBodyTaggedData(Stream outputStream) : ITaggedData<Stream>
{
    public Stream Data => outputStream;

    public SidechannelTag Tag { get; set; } = new SidechannelTag()
    {
        IsTainted = false,
        Stamp = 0,
        TaintCondition = SidechannelState.Always,
    };
}
