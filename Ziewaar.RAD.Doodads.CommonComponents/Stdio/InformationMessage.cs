using TtLog.LogFormats;

namespace Ziewaar.RAD.Doodads.CommonComponents.Stdio;

[Category("Diagnostics & Debug")]
[Title("Write an information message and optionally some interaction context to console/logfile")]
[Description("This is cool in prod.")]
[ShortNames("msg")]
public class InformationMessage : LogMessage
{
    protected override LogLevel Level { get; } = LogLevel.Info;
}
