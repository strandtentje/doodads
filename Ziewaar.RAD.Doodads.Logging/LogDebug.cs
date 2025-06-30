#nullable enable
#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.Logging;

public class LogDebug : UnleveledLog
{
    protected override void WriteLog(ILoggerWrapper wrapper, string text) => wrapper.Debug(text);
}
