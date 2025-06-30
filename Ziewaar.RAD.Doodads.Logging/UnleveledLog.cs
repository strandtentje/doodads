#nullable enable
#pragma warning disable 67
using System;
using System.ComponentModel;
using System.Linq;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;
using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

namespace Ziewaar.RAD.Doodads.Logging;

public abstract class UnleveledLog : IService
{
    [PrimarySetting("Fixed log text. Use < to print memory and continue, use > to sink text from OnThen")]
    private readonly UpdatingPrimaryValue LogTextConst = new();
    private string? FixedLogText;

    [EventOccasion("Sink log text from here, if no fixed text was provided. Otherwise, continue here.")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [EventOccasion("Likely happens when no LogToFile was configured.")]
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, LogTextConst).IsRereadRequired(out string? fixedLogText))
        {
            this.FixedLogText = fixedLogText;
        }

        if (!interaction.TryGetClosest<LoggerInteraction>(out var li) || li == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "configure the loggin first, using LogToFile for example"));
            return;
        }

        if (this.FixedLogText == null || this.FixedLogText.Count(x => !char.IsWhiteSpace(x)) == 0)
        {
            var tsi = new TextSinkingInteraction(interaction);
            OnThen?.Invoke(this, tsi);
            WriteLog(li.Logging, tsi.ReadAllText());
        } else
        {
            switch(FixedLogText.ToLower().Trim())
            {
                case "<":
                    WriteLog(li.Logging, interaction.Register.ToString());
                    OnThen?.Invoke(this, new LoggerInteraction(interaction, li.Logging));
                    break;
                case ">":
                    var tsi = new TextSinkingInteraction(interaction);
                    OnThen?.Invoke(this, tsi);
                    WriteLog(li.Logging, tsi.ReadAllText());
                    break;
                default:
                    WriteLog(li.Logging, FixedLogText);
                    OnThen?.Invoke(this, new LoggerInteraction(interaction, li.Logging));
                    break;
            }
        }
    }
    protected abstract void WriteLog(ILoggerWrapper wrapper, string text);
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, new CommonInteraction(source));
}
