using Ziewaar.RAD.Doodads.CoreLibrary;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

namespace Ziewaar.RAD.Doodads.AdvancedFilesystem;
[Category("System & IO")]
[Title("Get list of symlinks in a directory")]
[Description("""
             Provided a directory, gets symlinks in there and stuffs it in memory with from-to pairs.
             """)]
public class QuerySymlinks : IService
{
    [PrimarySetting("Repeat name to use with continue")]
    private readonly UpdatingPrimaryValue RepeatNameConstant = new();
    private string? CurrentRepeatName;

    public event CallForInteraction? LinkParentDir;
    [EventOccasion("When a file link was found")]
    public event CallForInteraction? OnThen;
    [EventOccasion("When a directory link was found")]
    public event CallForInteraction? OnElse;
    [EventOccasion("When the directory didn't exist or the repeat name wasn't right")]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, RepeatNameConstant).IsRereadRequired(out string? repeatName))
            this.CurrentRepeatName = repeatName;
        var tsi = new TextSinkingInteraction(interaction);
        LinkParentDir?.Invoke(this, tsi);
        var candidateDirPath = tsi.ReadAllText();
        var dirInfo = new DirectoryInfo(candidateDirPath);
        if (!dirInfo.Exists
            || string.IsNullOrWhiteSpace(CurrentRepeatName))
        {
            OnException?.Invoke(this, new CommonInteraction(interaction,
                "Existing directory path required for this"));
            return;
        }

        var links = SymlinkRepository.Instance.ListSymlinks(dirInfo.FullName);
        var repeater = new RepeatInteraction(this.CurrentRepeatName, interaction);
        foreach (var item in links)
        {
            repeater.IsRunning = false;
            var linkInformation = new SwitchingDictionary(["from", "to"], key => key switch
            {
                "from" => item.LinkPath,
                "to" => item.TargetPath,
                _ => throw new KeyNotFoundException(),
            });
            if (item.IsDirectory)
            {
                OnElse?.Invoke(this, new CommonInteraction(repeater, register: item.LinkPath, memory: linkInformation));
            }
            else
            {
                OnThen?.Invoke(this, new CommonInteraction(repeater, register: item.LinkPath, memory: linkInformation));
            }
            if (!repeater.IsRunning)
                return;
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
