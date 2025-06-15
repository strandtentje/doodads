#nullable enable
namespace Ziewaar.RAD.Doodads.CoreLibrary.Data;

public static class GlobalStopwatch
{
    public static readonly Stopwatch Instance = Stopwatch.StartNew();
}