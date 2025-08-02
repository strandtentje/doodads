#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services;
#nullable enable

[Category("Http Status")]
[Title("Http redirect to")]
[Description("""
             Does a 307 Temporary Redirect to the path sunk into OnThen
             """)]
public class HttpRedirect : IService
{
    [EventOccasion("Sink redirect URL here.")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [EventOccasion("Likely happens when we're trying to redirect something other than a webserver.")]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (!interaction.TryGetClosest<HttpResponseInteraction>(out var httpResponse) || httpResponse == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "HTTP redirection requires a webserver to have caused all of this."));
            return;
        }

        TextSinkingInteraction tsi = new(interaction);
        OnThen?.Invoke(this, tsi);
        httpResponse.RedirectTo(tsi.ReadAllText());
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}