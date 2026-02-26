namespace Ziewaar.RAD.Doodads.CoreLibrary.Predefined
{
    public class RootInteraction(object register, SortedList<string, object> memory) : ISelfStartingInteraction
    {
        public IInteraction Stack => StopperInteraction.Instance;
        public object Register => register;
        public IReadOnlyDictionary<string, object> Memory { get; } = new RootMemory(memory);
    }
}