#nullable enable
#pragma warning disable 67
using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;

namespace Ziewaar.RAD.Doodads.Logging;

[Category("Logging")]
[Title("Log at Fatal Level")]
[Description("Write to Log at Fatal level")]
public class LogFatal : UnleveledLog
{
    protected override void WriteLog(ILoggerWrapper wrapper, string text) => wrapper.Fatal(text);
}
