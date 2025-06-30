#nullable enable
#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.Logging;

public class LogInfo : UnleveledLog
{
    protected override void WriteLog(ILoggerWrapper wrapper, string text) => wrapper.Information(text);
}
