#nullable enable
#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.Logging;

public class LogFatal : UnleveledLog
{
    protected override void WriteLog(ILoggerWrapper wrapper, string text) => wrapper.Fatal(text);
}
