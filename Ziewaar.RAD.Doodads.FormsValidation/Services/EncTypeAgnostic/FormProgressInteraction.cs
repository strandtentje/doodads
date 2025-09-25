using Ziewaar.RAD.Doodads.EnumerableStreaming.Readers;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic
{
    public class FormProgressInteraction(IInteraction stack, RootByteReader reader, SpeedMetric metric) : IInteraction
    {
        public IInteraction Stack => stack;
        public object Register => stack.Register;
        public IReadOnlyDictionary<string, object> Memory { get; } = new SwitchingDictionary(
        [
            "position", "length", "rate",
            "progress", "size", "speed", 
            "percentage", "state"
        ], key => key switch
        {
            "position" => reader.Cursor,
            "length" => reader.Limit,
            "rate" => (reader, metric).GetOrCalculateSpeed(),
        
            "progress" => reader.Cursor.ToByteSizeString(),
            "size" => reader.Limit.ToByteSizeString(),
            "speed" => $"{(reader, metric).GetOrCalculateSpeed().ToByteSizeString()}/s",
        
            "percentage" => ((reader.Cursor * 100) / reader.Limit),
            "state" => reader.ErrorState ?? (reader.AtEnd ? "ended" : "working"),
            _ => throw new KeyNotFoundException(),
        });
        public void Finish()
        {
            if (!reader.AtEnd)
                reader.ErrorState ??= "Forced finish";
        }
    }
}