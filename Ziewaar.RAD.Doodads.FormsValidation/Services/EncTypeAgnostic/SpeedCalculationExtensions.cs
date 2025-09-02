using Ziewaar.RAD.Doodads.FormsValidation.Services.Support.Streaming.Readers;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic;

public static class SpeedCalculationExtensions
{
    public static long GetOrCalculateSpeed(this (ILongPosition position, ILongSpeed speed) set, ulong msMinInterval = 250)
    {
        if (set.position is ICountingEnumerator<byte> ce && !ce.CanContinue())
            return 0;
        
        var currentMilliseconds = GlobalStopwatch.Instance.ElapsedMilliseconds;
        var currentPosition = set.position.Cursor;
        
        var advancedMilliseconds = currentMilliseconds - set.speed.PreviousMilliseconds;
        var advancedBytes = currentPosition - set.speed.PreviousPosition;
        if (advancedMilliseconds > (long)msMinInterval)
        {
            set.speed.LastCalculatedSpeed = (advancedBytes * 1000) / advancedMilliseconds;
            
            set.speed.PreviousMilliseconds = currentMilliseconds;
            set.speed.PreviousPosition = currentPosition;
        }
        return set.speed.LastCalculatedSpeed;
    }
}