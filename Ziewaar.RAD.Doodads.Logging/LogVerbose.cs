#nullable enable
#pragma warning disable 67
using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;

namespace Ziewaar.RAD.Doodads.Logging;

[Category("Logging")]
[Title("Log at Verbose Level")]
[Description("Write to Log at Verbose level")]
public class LogVerbose : UnleveledLog
{
    protected override void WriteLog(ILoggerWrapper wrapper, string text) => wrapper.Verbose(text);
}
