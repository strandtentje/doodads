#nullable enable
#pragma warning disable 67
using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;

namespace Ziewaar.RAD.Doodads.Logging;
[Category("Logging")]
[Title("Log at Debug Level")]
[Description("Write to Log at Debug level")]
public class LogDebug : UnleveledLog
{
    protected override void WriteLog(ILoggerWrapper wrapper, string text) => wrapper.Debug(text);
}
