#nullable enable
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using System.Threading;

namespace Ziewaar.RAD.Doodads.CommonComponents.Control;
[Category("Flow Control")]
[Title("Start the closest timer")]
[Description("Hook up a timer here to start it")]
public class StartTimer() : TimerCommandSender(TimerCommand.Start);
