namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Interactions.Payloads;

internal class PrefixSinkData(Stream targetStream, long stamp, SidechannelState when) : ITaggedData<Stream>
{
    public Stream Data => targetStream;
    public SidechannelTag Tag { get; set; } = new SidechannelTag()
    {
        IsTainted = false,
        Stamp = stamp,
        TaintCondition = when,
    };
}
