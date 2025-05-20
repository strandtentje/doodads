namespace Ziewaar.RAD.Doodads.CommonComponents.Control
{
    public class TimerCommandInteraction(IInteraction parent, TimerCommand command) : IInteraction
    {
        public IInteraction Parent => parent;
        public IReadOnlyDictionary<string, object> Variables => parent.Variables;
        public TimerCommand Command => command;
        public bool IsConsumed = false;
    }
}
