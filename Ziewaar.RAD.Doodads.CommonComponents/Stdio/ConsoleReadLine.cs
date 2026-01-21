#pragma warning disable 67
#nullable enable
using Ziewaar.Common.Aardvargs;

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
            var readLine = Console.ReadLine();
            var halves = readLine?.Split([' '], 2, StringSplitOptions.RemoveEmptyEntries);
            var command = halves?.ElementAtOrDefault(0) ?? "";
            var args = halves.ElementAtOrDefault(1)?.Split([' '], StringSplitOptions.RemoveEmptyEntries) ?? [];
            var parsedArgs = ArgParser.Parse(args);
            yield return repeater.AppendRegister(command).AppendMemory(parsedArgs.Options);
        }
    }
}
