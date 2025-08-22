namespace Ziewaar.RAD.Doodads.AdvancedFilesystem;
public sealed class SymlinkInfo
{
    public string LinkPath { get; }
    public string TargetPath { get; }
    public bool IsDirectory { get; }

    public SymlinkInfo(string linkPath, string targetPath, bool isDirectory)
    {
        LinkPath = linkPath;
        TargetPath = targetPath;
        IsDirectory = isDirectory;
    }

    public override string ToString() =>
        $"{(IsDirectory ? "[Dir]" : "[File]")} {LinkPath} -> {TargetPath}";
}
