namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Interactions.Payloads;

internal class RequestMethodTaggedData(string httpMethod) : ITaggedData<HttpMethod>
{
    public HttpMethod Data => HttpMethod.Parse(httpMethod);
    public SidechannelTag Tag { get; set; } = new SidechannelTag() { 
        IsTainted = false, 
        Stamp = 0, 
        TaintCondition = SidechannelState.Never 
    };
}