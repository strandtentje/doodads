#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.ModuleLoader.Services;

public abstract class ReturningService : IService
{
    public abstract event CallForInteraction? OnThen;
    public abstract event CallForInteraction? OnElse;
    public abstract event CallForInteraction? OnException;
    /// <summary>
    /// This function deals with the fact that modules can call other modules,
    /// and if control is returned to another module, and that other higher-up
    /// module tries to do a return, we don't return to that inner module invocation,
    /// but rather the actual invocation that belongs to the scope we're looking at.
    /// </summary>
    /// <param name="interaction"></param>
    /// <returns>A free calling interaction that belongs to the caller of this module</returns>
    protected static CallingInteraction? FindCallerOfCurrentScope(IService returner, IInteraction interaction, int skip)
    {
        if (skip < 0)
        {
            return null;
        }
        else if (interaction is CallingInteraction ci)
        {
            if (skip == 0)
                return ci;
            else
                return FindCallerOfCurrentScope(returner, ci.Stack, skip - 1);
        }
        else if (interaction is ReturningInteraction ri)
        {
            if (false && ri.Returner == returner)
            {
                return ri.Cause;
            }
            else
            {
                return FindCallerOfCurrentScope(returner, ri.Stack, skip + 1);
            }
        }
        else if (interaction is StopperInteraction)
        {
            return null;
        }
        else
        {
            return FindCallerOfCurrentScope(returner, interaction.Stack, skip);
        }
    }

    public abstract void Enter(StampedMap constants, IInteraction interaction);
    public abstract void HandleFatal(IInteraction source, Exception ex);
}
