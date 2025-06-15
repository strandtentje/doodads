#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.Control;
[Category("Flow Control")]
[Title("Start the closest timer")]
public class StartTimer() : TimerCommandSender(TimerCommand.Start);
