#nullable enable
#pragma warning disable 67
using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;

namespace Ziewaar.RAD.Doodads.Logging;

[Category("Logging")]
[Title("Log at Warning Level")]
[Description("Write to Log at Warning level")]
public class LogWarning : UnleveledLog
{
    protected override void WriteLog(ILoggerWrapper wrapper, string text) => wrapper.Warning(text);
}
