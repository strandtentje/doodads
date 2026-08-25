#pragma warning disable 67
using Define.Doodads.Expo.Timeline;
using Ziewaar.RAD.Doodads.CommonComponents.Filesystem.Iterating;
using static Ziewaar.RAD.Doodads.CommonComponents.Filesystem.DirectoryOperations.DeepRenameUtils;

namespace Ziewaar.RAD.Doodads.CommonComponents.Filesystem.DirectoryOperations;

public class AccomodateFileNumbering : BasicService
{
    public override void TryEnter(StampedMap constants, IInteraction interaction)
    {
        var workingDirectory = interaction.Register.ToString();
        if (string.IsNullOrWhiteSpace(workingDirectory))
            throw new BasicException("directory required for this");
        if (!Directory.Exists(workingDirectory))
            Directory.CreateDirectory(workingDirectory);

        FileInfo? reorderFile = null;
        int direction = 0;
        if (constants.NamedItems.TryGetValue("up", out var upVarCand))
        {
            if (upVarCand is not string upvar || string.IsNullOrWhiteSpace(upvar) ||
                !interaction.TryFindVariable(upvar, out object? upFileCand) ||
                upFileCand?.ToString() is not string upFile || string.IsNullOrWhiteSpace(upFile) ||
                !File.Exists(upFile))
                throw new BasicException("up was specified, but no file was wound");
            reorderFile = new FileInfo(upFile);
            direction = -1;
        }
        if (constants.NamedItems.TryGetValue("down", out var downVarCand))
        {
            if (downVarCand is not string downvar || string.IsNullOrWhiteSpace(downvar) ||
                !interaction.TryFindVariable(downvar, out object? downFileCand) ||
                downFileCand?.ToString() is not string downFile || string.IsNullOrWhiteSpace(downFile) ||
                !File.Exists(downFile))
                throw new BasicException("down was specified, but no file was wound");
            if (reorderFile != null || direction != 0)
                throw new BasicException("up and down cannot be both specified");
            reorderFile = new FileInfo(downFile);
            direction = 1;
        }

        var dirInfo = new DirectoryInfo(workingDirectory);
        var allFilesNumbered = dirInfo.GetFiles().OrderBy(x => x.Name).Aggregate(
            new RenumberingFileList(reorderFile, direction), (acc, fi) => acc.Append(fi), x => x.Build()).ToArray();
        var renumberedFiles = allFilesNumbered.Where(x => x.IsRenameNeeded).ToArray();

        foreach (var files in renumberedFiles)
            File.Move(files.OldPath, files.IntermediatePath);
        foreach (var files in renumberedFiles)
            File.Move(files.IntermediatePath, files.FinalPath);
    }


}

public class Fraction(int numerator, Denominator denom)
{
    public int NumberPrefix => numerator * denom.Scale;
    public int Numerator => numerator;
    public Denominator Denom => denom;
}

public class Denominator(int value)
{
    public int Value => value;
    public void Increment() => value++;

    public int Scale => field > 0 ? field : field = Value switch
    {
        < 10 => 100,
        < 20 => 50,
        < 50 => 20,
        < 100 => 100,
        < 200 => 50,
        < 500 => 20,
        < 1000 => 100,
        < 2000 => 50,
        < 5000 => 20,
        < 10000 => 100,
        < 20000 => 50,
        < 50000 => 20,
        < 100000 => 10,
        _ => throw new BasicException("denominator too big.")
    };
    public string Format => field != null ? field : field = Value switch
    {
        < 1000 => "000",
        < 10000 => "0000",
        < 100000 => "00000",
        _ => throw new BasicException("denominator too big"),
    };
}

public class RenumberingFileList(FileInfo? reorderFile, int direction) : List<RenumberingFile>
{
    private readonly List<RenumberingFile> Files = new();
    private readonly Denominator Denom = new Denominator(0);

    private RenumberingFile? ExchangesNumberWithNextMember = null;

    public RenumberingFileList Append(FileInfo newFileItem)
    {
        Denom.Increment();
        var newMember = new RenumberingFile(newFileItem, new Fraction(Denom.Value, Denom));
        if (this.ExchangesNumberWithNextMember != null)
        {
            Files.Remove(ExchangesNumberWithNextMember);

            Files.Add(new RenumberingFile(newMember.InfoBeforeRename, ExchangesNumberWithNextMember.ProposedNumber));
            Files.Add(new RenumberingFile(ExchangesNumberWithNextMember.InfoBeforeRename, newMember.ProposedNumber));

            this.ExchangesNumberWithNextMember = null;
        }
        else if (reorderFile?.FullName == newFileItem.FullName && direction == -1 &&
            Files.LastOrDefault() is RenumberingFile previouslyAddedMember)
        {
            Files.Remove(previouslyAddedMember);

            Files.Add(new RenumberingFile(newMember.InfoBeforeRename, previouslyAddedMember.ProposedNumber));
            Files.Add(new RenumberingFile(previouslyAddedMember.InfoBeforeRename, newMember.ProposedNumber));
        }
        else if (reorderFile?.FullName == newFileItem.FullName && direction == 1)
        {
            this.ExchangesNumberWithNextMember = newMember;
            Files.Add(newMember);
        } else
        {
            Files.Add(newMember);
        }
        return this;
    }
    public RenumberingFile[] Build() => Files.ToArray();
}

public class RenumberingFile(FileInfo info, Fraction fraction)
{
    public FileInfo InfoBeforeRename => info;
    #region Before Renumber
    public string OldPath => info.FullName;
    public string OldName => info.Name;
    public string WithoutNumber
    {
        get
        {
            if (field != null) return field;
            field = new string(OldName.SkipWhile(char.IsNumber).SkipWhile(char.IsSymbol).ToArray());
            return field;
        }
    }
    #endregion
    #region After Renumber
    public Fraction ProposedNumber => fraction;
    public string IntermediateName => field ??= $"inter-{fraction.Numerator}-{fraction.Denom.Value}-{Guid.NewGuid().ToString()}.inter.move";
    public string IntermediatePath => Path.Combine(info.Directory.FullName, IntermediateName);
    public string FinalName => $"{fraction.NumberPrefix.ToString(fraction.Denom.Format)}-{WithoutNumber}";
    public string FinalPath => Path.Combine(info.Directory.FullName, FinalName);
    #endregion
    public bool IsRenameNeeded => OldName != FinalName;
}

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
            }
            else
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
            Directory.Move(renameDir.directory.FullName, newPath);
            yield return (new DirectoryInfo(newPath), renameDir.oldSuffix);
        }

        foreach (var renameDir in namedRenameDirs)
        {
            InspectAndRenameDirectories(parent, renameDir.oldName, renameDir.newName);
        }
    }
}