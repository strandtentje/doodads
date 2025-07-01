#nullable enable
#pragma warning disable 67
using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;

namespace Ziewaar.RAD.Doodads.Logging;

[Category("Logging")]
[Title("Log at Error Level")]
[Description("Write to Log at Erro level")]
public class LogError : UnleveledLog
{
    protected override void WriteLog(ILoggerWrapper wrapper, string text) => wrapper.Error(text);
}
