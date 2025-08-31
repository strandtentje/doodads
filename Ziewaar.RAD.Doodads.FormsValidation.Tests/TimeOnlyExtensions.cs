namespace Ziewaar.RAD.Doodads.FormsValidation.Tests;
static class TimeOnlyExtensions
{
    public static TimeOnly TruncateMilliseconds(this TimeOnly t, int msPrecision)
        => new TimeOnly(t.Hour, t.Minute, t.Second, t.Millisecond);
}