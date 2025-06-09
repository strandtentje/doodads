namespace Ziewaar.RAD.Doodads.ModuleLoader.Services;
public class Return : IService
{
    public event EventHandler<IInteraction> OnError;
    public void Enter(ServiceConstants serviceConstants, IInteraction interaction)
    {
        if (FindCallerOfCurrentScope(interaction).TryGetClosest<CallingInteraction>(out var callingInteraction))
            callingInteraction.Continue(new ReturningInteraction(interaction, callingInteraction, serviceConstants));
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
            offset = candidateOffset.Cause.Parent;
        }

        return offset;
    }
}
