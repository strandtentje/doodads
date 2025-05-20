namespace Ziewaar.RAD.Doodads.CommonComponents.Control
{
    public abstract class TimerCommandSender(TimerCommand command) : IService
    {
        [NamedBranch]
        public event EventHandler<IInteraction> OnError;
        [NamedBranch]
        public event EventHandler<IInteraction> Continue;
        public void Enter(ServiceConstants serviceConstants, IInteraction interaction)
        {
            Continue?.Invoke(this, new TimerCommandInteraction(interaction, command));
        }
    }
}
