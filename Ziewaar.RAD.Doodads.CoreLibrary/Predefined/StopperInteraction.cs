namespace Ziewaar.RAD.Doodads.CoreLibrary.Predefined
{
    public class StopperInteraction : IInteraction
    {
        public static readonly StopperInteraction Instance = new();
        private StopperInteraction() { }
        [JsonIgnore]
        public IInteraction Stack => throw new ArgumentOutOfRangeException("This is the stopper interaction.");
        [JsonIgnore]
        public object Register => throw new ArgumentOutOfRangeException("This is the stopper interaction");
        [JsonIgnore]
        public IReadOnlyDictionary<string, object> Memory => EmptyReadOnlyDictionary.Instance;
    }
}