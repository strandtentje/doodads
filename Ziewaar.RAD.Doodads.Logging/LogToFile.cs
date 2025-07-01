#nullable enable
#pragma warning disable 67
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

namespace Ziewaar.RAD.Doodads.Logging;
[Category("Logging")]
[Title("Setup logging (to file)")]
[Description("Configure file for logging into using the Log Services")]
public class LogToFile : IService, IDisposable, ILoggerWrapper
{
    [PrimarySetting("""
        Path to log file. Include a `{ts}` to timestamp it. It will be 
        replaced with the current yyyyMMddHHmm
        """)]
    private readonly UpdatingPrimaryValue LogFileConstant = new();
    [NamedSetting("level", """
        Minimal log level
        Verbose, Debug, Information,
        Warning, Error, Fatal
        """)]
    private readonly UpdatingKeyValue LogLevelConstant = new("level");
    private string? CurrentLogFile;
    private Logger? CurrentLogger;
    private bool isDisposed;
    private LogEventLevel CurrentLogLevel = LogEventLevel.Information;

    [EventOccasion("When the log file is ready for writing")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [EventOccasion("Likely when no log file was configured")]
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, LogFileConstant).IsRereadRequired(out string? newLogFile) && newLogFile != null)
        {
            this.CurrentLogFile = newLogFile.Replace("{ts}", DateTime.Now.ToString("yyyyMMddHHmm"));            
            this.CurrentLogger?.Dispose();
            this.CurrentLogger = null;
        }
        if ((constants, LogLevelConstant).IsRereadRequired(out string? newLlc) && newLlc != null && 
            Enum.TryParse<LogEventLevel>(newLlc, out var result))
        {
            this.CurrentLogLevel = result;
        }
        if (this.CurrentLogFile == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Log file is required as primary constant"));
            return;
        }
        if (this.CurrentLogger == null)
            this.CurrentLogger = new LoggerConfiguration().MinimumLevel.Is(CurrentLogLevel).WriteTo.File(CurrentLogFile).CreateLogger();
        OnThen?.Invoke(this, new LoggerInteraction(interaction, this));        
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
    public void Dispose()
    {
        if (!isDisposed)
        {
            this.isDisposed = true;
            this.CurrentLogger?.Dispose();
        }
    }

    public void Fatal(string message)
    {
        if (!isDisposed) this.CurrentLogger?.Fatal(message);
    }
    public void Error(string message)
    {
        if (!isDisposed) this.CurrentLogger?.Error(message);
    }
    public void Warning(string message)
    {
        if (!isDisposed) this.CurrentLogger?.Warning(message);
    }
    public void Information(string message)
    {
        if (!isDisposed) this.CurrentLogger?.Information(message);
    }
    public void Debug(string message)
    {
        if (!isDisposed) this.CurrentLogger?.Debug(message);
    }
    public void Verbose(string message)
    {
        if (!isDisposed) this.CurrentLogger?.Verbose(message);
    }
}
