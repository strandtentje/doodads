namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services;
#pragma warning disable 67
[Category("Tokens & Cryptography")]
[Title("Random+Unique Token")]
[Description("""
             Generates a token that hopes to be both unique and hard to guess.
             """)]
public class GenerateToken : IService
{
    [EventOccasion("When the token is ready, it's in register here.")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [NeverHappens]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        OnThen?.Invoke(this, new CommonInteraction(interaction, ComponentCookie.CreateNew().ToString()));
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}