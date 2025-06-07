namespace Ziewaar.RAD.Doodads.CommonComponents.Stdio
{
    public class ConsoleOutput : IService
    {
        public event EventHandler<IInteraction> OnError;
        [DefaultBranch]
        public event EventHandler<IInteraction> Continue;
        public static Stream StandardOutput = Console.OpenStandardOutput();
        public void Enter(ServiceConstants serviceConstants, IInteraction interaction)
        {
            Continue?.Invoke(this,
                new ConsoleSinkInteraction(interaction, StandardOutput));

        }
    }
}
