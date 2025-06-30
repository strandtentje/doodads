using System.Collections.Generic;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;

namespace Ziewaar.RAD.Doodads.Logging;

public class LoggerInteraction(IInteraction interaction, ILoggerWrapper logToFile) : IInteraction
{
    public ILoggerWrapper Logging => logToFile;
    public IInteraction Stack => interaction;
    public object Register => interaction.Register;
    public IReadOnlyDictionary<string, object> Memory => interaction.Memory;
}