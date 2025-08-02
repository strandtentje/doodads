#nullable enable
namespace Ziewaar.RAD.Doodads.ModuleLoader.Services;
#pragma warning disable 67
[Category("Application Lifecycle")]
[Title("Dispose of every resource")]
[Description("""
             For all services that consume tangible resources, stop them and release the resources.
             This doesn't kill the app per say but this is hard to recover from; typically 
             used in conjunction with EnvironmentExit
             """)]
public class ShutdownAll : IService
{
    [EventOccasion("After disposal happened")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [NeverHappens]
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        TypeRepository.Instance.Dispose();
        OnThen?.Invoke(this, interaction);
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}