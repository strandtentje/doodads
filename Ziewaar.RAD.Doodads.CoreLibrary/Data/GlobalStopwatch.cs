#nullable enable
using System.Diagnostics;

namespace Ziewaar.RAD.Doodads.CoreLibrary.Data;

public static class GlobalStopwatch
{
    public static readonly Stopwatch Instance = Stopwatch.StartNew();
}