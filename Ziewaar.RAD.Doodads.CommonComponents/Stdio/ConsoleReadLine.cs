#pragma warning disable 67

namespace Ziewaar.RAD.Doodads.CommonComponents.Stdio;

[Category("System & IO")]
[Title("Read console lines")]
[Description("Pops out lines typed into the console")]
public class ConsoleReadLine : IteratingService
{
    protected override bool RunElse { get; }

    protected override IEnumerable<IInteraction> GetItems(StampedMap constants, IInteraction repeater)
    {
        while(true)
        {
            var readLine = Console.ReadLine()?.Trim();
            yield return repeater.AppendRegister(readLine ?? "");
        }
    }
}
