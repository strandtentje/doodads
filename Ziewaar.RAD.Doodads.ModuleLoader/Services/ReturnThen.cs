#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.ModuleLoader.Services;
[Category("Call Definition Return")]
public class ReturnThen : ReturningService
{
    public override event CallForInteraction? OnThen;
    public override event CallForInteraction? OnElse;
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