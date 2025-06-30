#nullable enable
#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.Logging;

public class LogVerbose : UnleveledLog
{
    protected override void WriteLog(ILoggerWrapper wrapper, string text) => wrapper.Verbose(text);
}
