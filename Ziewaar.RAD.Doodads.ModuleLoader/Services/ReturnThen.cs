#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.ModuleLoader.Services;
[Category("Call Definition Return")]
[Title("Return to the OnThen of the invoking Call")]
[Description("""
    Returns control to the caller, on either the OnThen, or the OnElse branch. 
    """)]
public class ReturnThen : ReturningService
{
    [NeverHappens]
    public override event CallForInteraction? OnThen;
    [NeverHappens]
    public override event CallForInteraction? OnElse;
    [EventOccasion("""
        When some sort of infinite return loop was created. This means you're trying to do big brained stuff.
        Don't do big brained stuff.
        """)]
    public override event CallForInteraction? OnException;

    public override void Enter(StampedMap constants, IInteraction interaction)
    {
        if (FindCallerOfCurrentScope(this, interaction, 0) is CallingInteraction ci)
            ci.InvokeOnThen(new ReturningInteraction(this, interaction, ci, constants.NamedItems));
        else
            OnException?.Invoke(this, new CommonInteraction(interaction, "illegal double return"));
    }
    public override void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}