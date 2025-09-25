using Serilog;
using Serilog.Events;
using System.Diagnostics;

namespace Ziewaar.RAD.Doodads.Cryptography;

public class SerilogTraceListener : TraceListener
{
    private readonly ILogger _logger;
    private readonly LogEventLevel _logLevelOverride;

    public SerilogTraceListener(ILogger logger, LogEventLevel logLevelOverride = LogEventLevel.Debug)
    {
        _logger = logger;
        _logLevelOverride = logLevelOverride;
    }

    public override void Write(string? message)
    {
        // You can ignore or log raw Write calls as-is
    }

    public override void WriteLine(string? message)
    {
        if (!string.IsNullOrWhiteSpace(message))
            _logger.Write(_logLevelOverride, "[Trace] {Message}", message);
    }

    public override void TraceEvent(TraceEventCache? eventCache, string source, TraceEventType eventType, int id,
        string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        var logger = _logger.ForContext("TraceSource", source)
            .ForContext("TraceEventType", eventType.ToString());

        logger.Write(_logLevelOverride, "[{Source}] {Message}", source, message);
    }

    public static TraceSource CreateDebug<TType>(ILogger? logger)
    {
        var trace = new TraceSource(typeof(TType).FullName ?? "TraceSource", SourceLevels.All);
        trace.Listeners.Clear();
        if (logger != null)
            trace.Listeners.Add(new SerilogTraceListener(logger));
        return trace;
    }
}