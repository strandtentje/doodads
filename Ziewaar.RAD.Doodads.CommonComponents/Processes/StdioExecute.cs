#nullable enable
#pragma warning disable 67
using System.Diagnostics;
using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;

namespace Ziewaar.RAD.Doodads.CommonComponents.Processes;

[Category("System & IO")]
[Title("Run program and access stdio via source and sink")]
[Description("""
    Runs a command and exposes stdio via the source and sink streams via OnThen
    """)]
public class StdioExecute : IService
{
    [PrimarySetting("Name for continue block")]
    private readonly UpdatingPrimaryValue ContinueNameConstant = new();
    private string? ContinueName;

    [EventOccasion("Sink command text here")]
    public event CallForInteraction? Command;
    [EventOccasion("Sink argument text here")]
    public event CallForInteraction? Arguments;
    [EventOccasion("When an error line was received on stderr")]
    public event CallForInteraction? ErrorLine;

    [EventOccasion("When the process was started an stdio is available; use Continue keep on looping stdio")]
    public event CallForInteraction? OnThen;
    [EventOccasion("When the stdio loop died; use Continue here to prevent the process from dying. Not recommended")]
    public event CallForInteraction? OnElse;
    [EventOccasion("Likely when the continue name was not there.")]
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, ContinueNameConstant).IsRereadRequired(out string? contineNameCandidate))
            this.ContinueName = contineNameCandidate;
        if (string.IsNullOrWhiteSpace(ContinueName) || ContinueName == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Continue name required"));
            return;
        }

        var commandInteraction = new TextSinkingInteraction(interaction);
        Command?.Invoke(this, commandInteraction);
        var commandText = commandInteraction.ReadAllText();

        var argumentInteraction = new TextSinkingInteraction(interaction);
        Arguments?.Invoke(this, argumentInteraction);
        var argumentText = argumentInteraction.ReadAllText();

        var psi = new ProcessStartInfo
        {
            FileName = commandText,
            Arguments = argumentText,
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };

        using var newProcess = Process.Start(psi);
        newProcess.ErrorDataReceived += (s, e) => ErrorLine?.Invoke(this, new CommonInteraction(interaction, e.Data));
        newProcess.BeginErrorReadLine();
        var binarySourceInteraction = new StandardOutputInteraction(interaction, newProcess.StandardOutput.BaseStream);
        var binarySinkInteraction = new StandardInputInteraction(binarySourceInteraction, newProcess.StandardInput.BaseStream);

        (binarySinkInteraction, this.ContinueName).RunCancellable(repeatInteraction =>
        {
            repeatInteraction.IsRunning = true;

            while (repeatInteraction.IsRunning && !repeatInteraction.IsCancelled())
            {
                repeatInteraction.IsRunning = false;
                OnThen?.Invoke(this, repeatInteraction);
                if (!newProcess.HasExited) continue;
                repeatInteraction.Cancel();
            }

            OnElse?.Invoke(this, repeatInteraction);
            if (repeatInteraction.IsRunning && !newProcess.HasExited)
                newProcess.Kill();
        });
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
