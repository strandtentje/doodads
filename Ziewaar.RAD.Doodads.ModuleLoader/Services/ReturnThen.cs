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
        if (FindCallerOfCurrentScope(interaction, 0) is CallingInteraction ci)
            ci.InvokeOnThen(new ReturningInteraction(interaction, ci, constants.NamedItems));
        else
            OnException?.Invoke(this, new CommonInteraction(interaction, "illegal double return"));
    }
    /// <summary>
    /// This function deals with the fact that modules can call other modules,
    /// and if control is returned to another module, and that other higher-up
    /// module tries to do a return, we don't return to that inner module invocation,
    /// but rather the actual invocation that belongs to the scope we're looking at.
    /// </summary>
    /// <param name="interaction"></param>
    /// <returns>A free calling interaction that belongs to the caller of this module</returns>
    private static CallingInteraction? FindCallerOfCurrentScope(IInteraction interaction, int skip)
    {
        if (interaction is CallingInteraction ci)
        {
            if (skip == 0)
                return ci;
            else if (skip > 0)
                return FindCallerOfCurrentScope(ci.Stack, skip - 1);
            else
                return null;
        }
        else if (interaction is ReturningInteraction ri)
        {
            return FindCallerOfCurrentScope(ri.Stack, skip + 1);
        }
        else if (interaction is StopperInteraction)
        {
            return null;
        }
        else
        {
            return FindCallerOfCurrentScope(interaction.Stack, skip);
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}