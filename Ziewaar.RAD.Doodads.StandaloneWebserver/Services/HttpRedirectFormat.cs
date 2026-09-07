#pragma warning disable 67
using Define.Doodads.Expo.Timeline;
using Ziewaar.RAD.Doodads.CommonComponents.TextTemplating;

namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services;

[Category("Http & Routing")]
[Title("Http redirect to formatted")]
[Description("""
             Does a 307 Temporary Redirect to the path template in primary constant
             """)]
[ShortNames("fredir")]
public class HttpRedirectFormat : BasicService
{
    private StampedMap? Constants;
    private readonly Format FormatService = new();
    private readonly HttpRedirect RedirectService = new();
    public HttpRedirectFormat()
    {
        RedirectService.OnThen += (s, e) => FormatService.Enter(Constants!, e);
        RedirectService.OnException += (s, e) => throw new BasicException(e.Register.ToString());
    }
    public override void TryEnter(StampedMap constants, IInteraction interaction)
    {
        this.Constants = constants;
        RedirectService.Enter(this.Constants, interaction);
    }
}