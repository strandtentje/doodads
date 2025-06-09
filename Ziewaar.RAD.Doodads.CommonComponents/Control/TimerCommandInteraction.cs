namespace Ziewaar.RAD.Doodads.CommonComponents.Control
{
    public class TimerCommandInteraction(IInteraction parent, TimerCommand command) : IInteraction
    {
        public TimerCommand Command => command;
        public bool IsConsumed = false;
        public IInteraction Stack => parent;
        public object Register => parent.Register;
        public IReadOnlyDictionary<string, object> Memory => parent.Memory;
    }
}
