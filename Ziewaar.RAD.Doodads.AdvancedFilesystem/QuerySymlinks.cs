using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;
using Ziewaar.RAD.Doodads.CoreLibrary;

namespace Ziewaar.RAD.Doodads.AdvancedFilesystem;

public class QuerySymlinks : IService
{
    private readonly UpdatingPrimaryValue RepeatNameConstant = new();
    private string? CurrentRepeatName;

    public event CallForInteraction? LinkParentDir;

    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
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
