#pragma warning disable 67
using Define.Doodads.Expo.Timeline;
using static Ziewaar.RAD.Doodads.CommonComponents.Filesystem.DirectoryOperations.DeepRenameUtils;

namespace Ziewaar.RAD.Doodads.CommonComponents.Filesystem.DirectoryOperations;

public class AccomodateDirectoryNumbering : BasicService
{
    public override void TryEnter(StampedMap constants, IInteraction interaction)
    {
        var directory = new DirectoryInfo(interaction.Register?.ToString() ?? throw new BasicException("directory requried"));
        List<DirectoryInfo> numberedDirectories = new();
        List<DirectoryInfo> numberlessDirectories = new();
        var allDirectories = directory.GetDirectories().OrderBy(x => x.Name).ToArray();
        int greatestNumber = 0;
        foreach (var item in allDirectories)
        {
            if (item.TryGetNumberPrefix(out var pfx, out var rem))
            {
                greatestNumber = Math.Max(greatestNumber, int.Parse(pfx));
                numberedDirectories.Add(item);
            } else
            {
                numberlessDirectories.Add(item);
            }
        }
        if (!constants.NamedItems.TryGetValue("force", out var fv) || fv is not bool fvb || !fvb)
        {
            if (greatestNumber < 900 && numberlessDirectories.Count == 0)
                return;
            if (greatestNumber < (900 - numberlessDirectories.Count * 10))
            {
                var fti = DetermineFilesToInspect(directory, constants.PrimaryConstant);
                RenumberStartingAt(parent: directory, inspect: fti, startAt: greatestNumber + 10, interval: 10,
                    toRenumber: numberlessDirectories.Select(x => (x, default(string))));
                return;
            }
        }

        var interval = 800.0 / allDirectories.Length;
        interval /= 10.0;
        interval = Math.Floor(interval);
        interval *= 10.0;
        var roundInterval = (int)interval;

        var oldToTemp = allDirectories.Select(x => (x, (string?)new string(Guid.NewGuid().ToString().Where(char.IsLetterOrDigit).ToArray())));
        var oldToTempFiles = DetermineFilesToInspect(directory, constants.PrimaryConstant);
        var renamed = RenumberStartingAt(directory, oldToTempFiles, 100, roundInterval, oldToTemp);

        var tempToNew = renamed.Select(x => (x.directory, (string?)x.oldSuffix));
        var tempToNewFiles = DetermineFilesToInspect(directory, constants.PrimaryConstant);
        RenumberStartingAt(directory, tempToNewFiles, 100, roundInterval, tempToNew);
    }

    private IEnumerable<(DirectoryInfo directory, string oldSuffix)> RenumberStartingAt(
        DirectoryInfo parent, FileInfo[] inspect, int startAt, int interval,
        IEnumerable<(DirectoryInfo directory, string? newName)> toRenumber)
    {
        List<(DirectoryInfo directory, string oldName, string oldSuffix, string newName)> namedRenameDirs = new();

        foreach (var item in toRenumber)
        {
            var suffix = item.directory.GetAfterNumberPrefix();
            namedRenameDirs.Add((
                item.directory, 
                item.directory.Name, 
                suffix,
                $"{startAt:000}-{item.newName ?? suffix}"));
            startAt += interval;
        }

        foreach (var renameDir in namedRenameDirs)
            InspectAndReplaceInFiles(inspect, renameDir.oldName, renameDir.newName);

        foreach (var renameDir in namedRenameDirs)
        {
            var newPath = Path.Combine(parent.FullName, renameDir.newName);
            DirectoryOperations.Move(renameDir.directory.FullName, newPath);
            yield return (new DirectoryInfo(newPath), renameDir.oldSuffix);
        }

        foreach (var renameDir in namedRenameDirs)
        {
            InspectAndRenameDirectories(parent, renameDir.oldName, renameDir.newName);
        }
    }
}