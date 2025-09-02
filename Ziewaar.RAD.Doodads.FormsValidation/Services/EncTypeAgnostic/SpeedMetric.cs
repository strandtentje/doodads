namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic;

public class SpeedMetric : ILongSpeed
{
    long ILongSpeed.PreviousPosition { get; set; } = 0;
    long ILongSpeed.PreviousMilliseconds { get; set; } = 0;
    long ILongSpeed.LastCalculatedSpeed { get; set; } = 0;
    public long LastCalculatedSpeed => ((ILongSpeed)this).LastCalculatedSpeed;
}