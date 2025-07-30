namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services;
#pragma warning disable 67
public class ControlCommandInstanceProvider<TCommandEnum>
    where TCommandEnum : struct, Enum, IConvertible
{
    private IControlCommandReceiver<TCommandEnum>? CurrentInstance = null;

    public bool TryHandleCommand<TResult>(
        IInteraction commandSource,
        TCommandEnum command,
        Func<IControlCommandReceiver<TCommandEnum>>? factory,
        [NotNullWhen(true)] out TResult? result)
        where TResult : IControlCommandReceiver<TCommandEnum>
    {
        result = default(TResult);
        if (factory != null)
            CurrentInstance ??= factory();
        if ( // either there was no command
            !commandSource.TryGetClosest<IControlCommandInteraction>(out var commandInteraction, x => x.CanApply(command)) ||
            commandInteraction == null ||
            // it was not equal to the command we want to handle
            !commandInteraction.Command.Equals(command) ||
            // or the command is already set
            CurrentInstance?.CurrentState.Equals(command) == true)
            // then we see no need to handle this command.
            return false;
        result = (TResult?)CurrentInstance;
        return result != null;
    }

    public void Reset()
    {
        try
        {
            CurrentInstance?.Dispose();
        }
        catch (Exception ex)
        {
            GlobalLog.Instance?.Error(ex, "Failed to reset {type}", CurrentInstance?.GetType().Name ?? "null");
        }
        finally
        {
            CurrentInstance = null;
        }
    }
}