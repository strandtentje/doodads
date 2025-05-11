namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Interactions.Payloads;

public class RequestBodyTaggedData(Stream inputStream) : ITaggedData<Stream>
{
    public Stream Data => inputStream;

    public SidechannelTag Tag { get; set; } = new SidechannelTag()
    {
        IsTainted = false,
        Stamp = 0,
        TaintCondition = SidechannelState.Never,
    };
}
