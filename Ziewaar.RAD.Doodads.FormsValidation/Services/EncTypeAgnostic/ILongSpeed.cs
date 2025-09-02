namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic;

public interface ILongSpeed
{
    long PreviousPosition { get; set; }
    long PreviousMilliseconds { get; set; }
    long LastCalculatedSpeed { get; set; }
}