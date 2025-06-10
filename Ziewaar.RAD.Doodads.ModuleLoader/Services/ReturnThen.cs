#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.ModuleLoader.Services;
public class ReturnThen : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (FindCallerOfCurrentScope(interaction).TryGetClosest<CallingInteraction>(out var callingInteraction))
            callingInteraction!.InvokeOnThen(new ReturningInteraction(interaction, callingInteraction, constants.NamedItems));
    }
    /// <summary>
    /// This function deals with the fact that modules can call other modules,
    /// and if control is returned to another module, and that other higher-up
    /// module tries to do a return, we don't return to that inner module invocation,
    /// but rather the actual invocation that belongs to the scope we're looking at.
    /// </summary>
    /// <param name="interaction"></param>
    /// <returns>A free calling interaction that belongs to the caller of this module</returns>
    private static IInteraction FindCallerOfCurrentScope(IInteraction interaction)
    {
        var offset = interaction;
        while (offset.TryGetClosest<ReturningInteraction>(out var candidateOffset))
        {
            offset = candidateOffset!.Cause.Stack;
        }

        return offset;
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}