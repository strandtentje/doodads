namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Interactions.Payloads;

public class RespondingTaggedCookies(CookieCollection cookies) : ITaggedData<CookieCollection>
{
    public CookieCollection Data => cookies;
    public SidechannelTag Tag { get; set; } = new SidechannelTag()
    {
        IsTainted = false,
        Stamp = 0,
        TaintCondition = SidechannelState.Always,
    };
}
