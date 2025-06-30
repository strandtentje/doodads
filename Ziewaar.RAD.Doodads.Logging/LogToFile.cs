#nullable enable
#pragma warning disable 67
using Serilog;
using Serilog.Core;
using System;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

namespace Ziewaar.RAD.Doodads.Logging;

public class LogToFile : IService, IDisposable, ILoggerWrapper
{
    private readonly UpdatingPrimaryValue LogFileConstant = new();
    private string? CurrentLogFile;
    private Logger? CurrentLogger;
    private bool isDisposed;

    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, LogFileConstant).IsRereadRequired(out string? newLogFile))
        {
            this.CurrentLogFile = newLogFile;            
            this.CurrentLogger?.Dispose();
            this.CurrentLogger = null;
        }
        if (this.CurrentLogFile == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Log file is required as primary constant"));
            return;
        }
        if (this.CurrentLogger == null)
            this.CurrentLogger = new LoggerConfiguration().WriteTo.File(CurrentLogFile).CreateLogger();
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
