namespace Ziewaar.RAD.Doodads.Logging;

public interface ILoggerWrapper
{
    void Debug(string message);
    void Error(string message);
    void Fatal(string message);
    void Information(string message);
    void Verbose(string message);
    void Warning(string message);
}