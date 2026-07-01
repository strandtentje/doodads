#pragma warning disable 67

using System.Collections.Concurrent;

namespace Ziewaar.RAD.Doodads.CommonComponents.Stdio;

[Category("System & IO")]
[Title("Read console lines")]
[Description("Pops out lines typed into the console")]
public class ConsoleReadLine : IteratingService
{
    protected override bool RunElse { get; }

    protected override IEnumerable<IInteraction> GetItems(StampedMap constants, IInteraction repeater)
    {
        if (repeater.TryGetCustom<InjectedConsole>(out var inj) && inj != null)
        {
            while(!inj.Commands.IsCompleted)
            {
                if (inj.Commands.TryTake(out string item, 300))
                {
                    yield return repeater.AppendRegister(item);
                }
            }
        }
        else
        {
            while (true)
            {
                var readLine = Console.ReadLine()?.Trim();
                yield return repeater.AppendRegister(readLine ?? "");
            }
        }
    }
}

public class InjectedConsole
{
    public readonly BlockingCollection<string> Commands = new BlockingCollection<string>(new ConcurrentQueue<string>());
}