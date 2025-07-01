#nullable enable
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using System.Threading;

namespace Ziewaar.RAD.Doodads.CommonComponents.Control;
[Category("Flow Control")]
[Title("Start the closest timer")]
public class StartTimer() : TimerCommandSender(TimerCommand.Start);
