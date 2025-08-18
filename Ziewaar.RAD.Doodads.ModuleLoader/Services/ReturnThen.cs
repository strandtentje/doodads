#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.ModuleLoader.Services;

[Category("Call Definition Return")]
[Title("Return to the OnThen of the invoking Call")]
[Shorthand("[:CONSTANTS]")]
[Description("""
             Returns control to the caller, on either the OnThen, or the OnElse branch. 
             """)]
public class ReturnThen : ReturningService
{
    [PrimarySetting("If set, will put this value in register")]
    private readonly UpdatingPrimaryValue OverrideRegisterValueConstant = new();

    private object? OverrideValue;

    [NeverHappens] public override event CallForInteraction? OnThen;
    [NeverHappens] public override event CallForInteraction? OnElse;

    [EventOccasion("""
                   When some sort of infinite return loop was created. This means you're trying to do big brained stuff.
                   Don't do big brained stuff.
                   """)]
    public override event CallForInteraction? OnException;

    public override void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, OverrideRegisterValueConstant).IsRereadRequired(out object? toOverride) && toOverride is string or decimal)
        {
            this.OverrideValue = toOverride;
        }

        if (FindCallerOfCurrentScope(this, interaction, 0) is CallingInteraction ci)
            ci.InvokeOnThen(new ReturningInteraction(this, OverrideValue ?? interaction.Register, interaction, ci, constants.NamedItems));
        else
            OnException?.Invoke(this, new CommonInteraction(interaction, "illegal double return"));
    }

    public override void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}