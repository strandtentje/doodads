#nullable enable
#pragma warning disable 67
using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;

namespace Ziewaar.RAD.Doodads.Logging;

[Category("Logging")]
[Title("Log at Info Level")]
[Description("Write to Log at Info level")]
public class LogInfo : UnleveledLog
{
    protected override void WriteLog(ILoggerWrapper wrapper, string text) => wrapper.Information(text);
}
