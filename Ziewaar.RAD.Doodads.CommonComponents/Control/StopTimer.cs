namespace Ziewaar.RAD.Doodads.CommonComponents.Control;
[Category("Flow Control")]
[Title("Stop the closest timer")]
[Description("""Hook this up to a timer to start it""")]
public class StopTimer() : TimerCommandSender(TimerCommand.Stop);