#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.Filesystem;

[Category("System & IO")]
[Title("remove line read from file")]
[Description("From LinesFromFile, remove a line.")]
public class RemoveLine : IService
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
        fli.SkipLine = true;
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
