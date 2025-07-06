#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services;
#nullable enable

public class HttpRedirect : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
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