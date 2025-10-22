#nullable enable
using Ziewaar;

namespace Ziewaar.RAD.Doodads.CommonComponents.Filesystem.Lines;

[Category("System & IO")]
[Title("Append line to line read from file")]
[Description("From LinesFromFile, append a line.")]
public class AppendLine : IService
{
    [EventOccasion("Sink changed line here")]
    public event CallForInteraction OnThen;
    [NeverHappens]
    public event CallForInteraction OnElse;
    [EventOccasion("Likely when no lines were read from file.")]
    public event CallForInteraction OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (!interaction.TryGetClosest<FileLineInteraction>(out var fli) ||
            fli == null)
        {
            OnException?.Invoke(this, interaction.AppendRegister("no file line"));
            return;
        }
        var tsi = new TextSinkingInteraction(interaction);
        OnThen?.Invoke(this, tsi);
        fli.LineAfter = tsi.ReadAllText();
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
