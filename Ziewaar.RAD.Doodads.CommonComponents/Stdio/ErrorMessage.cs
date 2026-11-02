using TtLog;
using TtLog.LogFormats;

namespace Ziewaar.RAD.Doodads.CommonComponents.Stdio;

[Category("Diagnostics & Debug")]
[Title("Write an error message and optionally some interaction context to console/logfile")]
[Description("This is cool in prod.")]
public class ErrorMessage : LogMessage
{
    protected override LogLevel Level { get; } = LogLevel.Fail;
}

public class WarnMessage : LogMessage
{
    protected override LogLevel Level { get; } = LogLevel.Warn;
}

public class CoolMessage : LogMessage
{
    protected override LogLevel Level { get; } = LogLevel.Cool;
}