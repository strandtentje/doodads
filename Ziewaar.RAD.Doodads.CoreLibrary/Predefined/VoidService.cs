#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.CoreLibrary.Predefined;
[Category("Does Nothing")]
[Title("It does absolutely nothing")]
[Description("""
             This service is responsible for 
              - nothing
             
             This may be very useful if you for example have
              - everything
             
             And you don't want that.
             """)]
public class VoidService : IService
{
    [NeverHappens]
    public event CallForInteraction? OnThen;
    [EventOccasion("For some reason it still does this, but why?")]
    public event CallForInteraction? OnElse;
    [EventOccasion("Can't do it wrong if you ain't doing it.")]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        OnElse?.Invoke(this, interaction);
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}