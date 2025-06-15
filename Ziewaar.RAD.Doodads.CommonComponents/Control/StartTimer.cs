#nullable enable
using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;

namespace Ziewaar.RAD.Doodads.CommonComponents.Control;
[Title("Start the closest timer")]
public class StartTimer() : TimerCommandSender(TimerCommand.Start);
