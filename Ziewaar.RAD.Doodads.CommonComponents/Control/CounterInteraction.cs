#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.Control;

public class CounterInteraction : IInteraction
{
    public decimal CounterValue;
    private readonly IInteraction Stack1;
    private readonly string CounterName;

    public CounterInteraction(IInteraction stack, string counterName, decimal counterValue)
    {
        Stack1 = stack;
        CounterName = counterName;
        CounterValue = counterValue;
        Memory = new SwitchingDictionary(
            [CounterName], key => key == CounterName ? CounterValue : throw new KeyNotFoundException());
    }

    public string Name => CounterName;
    public IInteraction Stack => Stack1;
    public object Register => Stack1.Register;
    public IReadOnlyDictionary<string, object> Memory { get; }

    public void Add(decimal currentIncrementValue) => CounterValue += currentIncrementValue;
}