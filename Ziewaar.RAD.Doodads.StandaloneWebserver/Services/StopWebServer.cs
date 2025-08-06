#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services;
[Category("Http & Routing")]
[Title("Cease underlying webserver")]
[Description("""
             When pointing this to a WebServer, it'll stop it.
             """)]
public class StopWebServer : IService
{
    [EventOccasion("Hook up the webserver to stop here.")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [NeverHappens]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        OnThen?.Invoke(this, new ServerCommandInteraction(interaction, ServerCommand.Stop));
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
