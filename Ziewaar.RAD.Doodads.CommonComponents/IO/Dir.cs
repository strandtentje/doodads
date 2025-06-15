#nullable enable
#pragma warning disable 67
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Ziewaar.RAD.Doodads.CommonComponents.IO;
[Category("Filesystem")]
[Title("Produces a list of directories, given the path currently in the Register.")]
[Description("""
             Dir will query filesystem for the directory path provided in the Register.
             The events are intended for finding out if the Directory exists, what its 
             subdirectories are, and what its files are.
             """)]
public class Dir : IService
{
    [PrimarySetting("Wildcard-enabled pattern to filter the files to be shown, ie *.txt or cheese.*")]
    private readonly UpdatingPrimaryValue FileSearchPatternConstant = new();
    [NamedSetting("filterdirs", "Wildcard-enabled pattern specifically to filter the directories")]
    private readonly UpdatingKeyValue DirSearchPattern = new("filterdirs");
    [EventOccasion("This puts a list of subdirectory paths in the register")]
    public event CallForInteraction? OnThen;
    [EventOccasion("This will happen after OnThen, and puts a list of file paths in the register")]
    public event CallForInteraction? OnElse;
    [EventOccasion("This will happen instead of OnThen/OnElse, in case the directory in the Register did not exist")]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        (constants, FileSearchPatternConstant).IsRereadRequired(() => "*", out var fileSearchPattern);
        (constants, DirSearchPattern).IsRereadRequired(() => "*", out var dirSearchPattern);
        fileSearchPattern ??= "*";
        dirSearchPattern ??= "*";

        var dirPath = interaction.Register.ToString();
        DirectoryInfo info = new DirectoryInfo(dirPath);

        if (!info.Exists)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "directory not found"));
            return;
        }

        if (OnThen != null)
            OnThen.Invoke(this,
                new CommonInteraction(interaction,
                    info.GetDirectories(dirSearchPattern, SearchOption.TopDirectoryOnly)));
        if (OnElse != null)
            OnElse?.Invoke(this,
                new CommonInteraction(interaction,
                    info.GetFiles(fileSearchPattern, SearchOption.TopDirectoryOnly)));
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}