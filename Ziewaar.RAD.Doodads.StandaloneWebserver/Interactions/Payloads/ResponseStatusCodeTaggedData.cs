namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Interactions.Payloads;

internal class ResponseStatusCodeTaggedData(HttpListenerResponse response) : ITaggedData<HttpStatusCodeChange>
{
    public HttpStatusCodeChange Data { get; } = new HttpStatusCodeChange(response);

    public SidechannelTag Tag { get; set; } = new SidechannelTag()
    {
        IsTainted = false,
        Stamp = 0,
        TaintCondition = SidechannelState.StampGreater,
    };
}
