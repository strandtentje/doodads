using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;

namespace Ziewaar.RAD.Doodads.ModuleLoader.Services.Reflection.Crud;
#pragma warning disable 67
#nullable enable
[Category("Reflection & Documentation")]
[Title("List all directories that contain running programs")]
[Description("""
             For the currently running doodads instance, this service enumerates 
             all open working directories / common ancestors.
             """)]
public class ProgramRootDirectories : IService
{
    private readonly UpdatingPrimaryValue RepeatNameConstant = new();
    private string? CurrentRepeatName;
    [EventOccasion("A list with 0 or more directory info's")]
    public event CallForInteraction? OnThen;
    [NeverHappens] public event CallForInteraction? OnElse;
    [NeverHappens] public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, RepeatNameConstant).IsRereadRequired(out string? repeatNameCandidate))
            this.CurrentRepeatName = repeatNameCandidate;
        if (string.IsNullOrWhiteSpace(this.CurrentRepeatName))
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Repeat name required"));
            return;
        }

        var allDirs = ProgramRepository.Instance.GetKnownPrograms()
            .Select(x => (x.Emitter.DirectoryInfo!.FullName, x.Emitter.DirectoryInfo));
        var sortedDistinctDirs =
            allDirs.Distinct(new FullNameComparer()).OrderBy(x => x.FullName);

        List<(string FullName, DirectoryInfo DirectoryInfo)> rootDirectories = new();

        var repeater = new RepeatInteraction(this.CurrentRepeatName, interaction);
        string? lastDirectory = null;

        using (var dirEnumerator = sortedDistinctDirs.GetEnumerator())
        {
            repeater.IsRunning = true;
            while (repeater.IsRunning & dirEnumerator.MoveNext())
            {
                if (lastDirectory == null ||
                    dirEnumerator.Current.FullName.StartsWith(lastDirectory, StringComparison.OrdinalIgnoreCase))
                    continue;
                repeater.IsRunning = false;
                OnThen?.Invoke(this, new CommonInteraction(repeater, dirEnumerator.Current.DirectoryInfo));
                lastDirectory = dirEnumerator.Current.DirectoryInfo.FullName;
            }
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}