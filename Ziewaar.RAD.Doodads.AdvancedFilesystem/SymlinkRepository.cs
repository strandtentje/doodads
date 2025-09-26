using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Ziewaar.RAD.Doodads.AdvancedFilesystem;
public sealed class SymlinkRepository
{
    private static readonly Lazy<SymlinkRepository> LazyInstance = new(() => new SymlinkRepository());
    public static SymlinkRepository Instance => LazyInstance.Value;
    private bool IsWindows { get; } = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    public void Create(string linkPath, string targetPath, bool isDirectory)
    {
        if (File.Exists(linkPath) || Directory.Exists(linkPath))
            throw new IOException($"Link path already exists: {linkPath}");

        if (IsWindows)
        {
            string linkType = isDirectory ? "/D" : "";
            string args = $"/C mklink {linkType} \"{linkPath}\" \"{targetPath}\"";
            RunCommand("cmd.exe", args);
        }
        else
        {
            string args = $"-s \"{targetPath}\" \"{linkPath}\"";
            RunCommand("ln", args);
        }
    }
    public string Read(string linkPath)
    {
        EnsureSymlink(linkPath);
        var resolved = new FileInfo(SymlinkHelper.ResolveSymlinkTarget(linkPath) ??
                                    throw new IOException("Could not resolve symlink target"));
        return resolved.FullName ?? throw new IOException("Could not resolve link target");
    }
    public void Upsert(string linkPath, string targetPath, bool isDirectory)
    {
        if (!File.Exists(linkPath) && !Directory.Exists(linkPath))
        {
            Create(linkPath, targetPath, isDirectory);
        }
        else if (SymlinkHelper.IsSymlink(linkPath))
        {
            Update(linkPath, targetPath, isDirectory);
        }
        else
        {
            throw new IOException($"Path exists and is not a symbolic link: {linkPath}");
        }
    }
    public void Update(string linkPath, string newTargetPath, bool isDirectory)
    {
        if (File.Exists(linkPath) || Directory.Exists(linkPath))
        {
            EnsureSymlink(linkPath);
            Delete(linkPath);
        }
        Create(linkPath, newTargetPath, isDirectory);
    }
    public void Delete(string linkPath)
    {
        EnsureSymlink(linkPath);

        if (File.Exists(linkPath))
            File.Delete(linkPath);
        else if (Directory.Exists(linkPath))
            Directory.Delete(linkPath);
    }
    public List<SymlinkInfo> ListSymlinks(string directoryPath)
    {
        var result = new List<SymlinkInfo>();

        foreach (var path in Directory.EnumerateFileSystemEntries(directoryPath))
        {
            if (SymlinkHelper.ResolveSymlinkTarget(path) is not string symlinkPathCandidate) continue;
            var target = new FileInfo(symlinkPathCandidate);
            var isDir = target.Attributes.HasFlag(FileAttributes.Directory);
            result.Add(new SymlinkInfo(path, target.FullName, isDir));
        }

        return result;
    }
    public void ClearSymlinks(string directoryPath)
    {
        foreach (var path in Directory.EnumerateFileSystemEntries(directoryPath))
        {
            if (SymlinkHelper.IsSymlink(path))
            {
                if (File.Exists(path))
                    File.Delete(path);
                else if (Directory.Exists(path))
                    Directory.Delete(path);
            }
        }
    }
    private static void EnsureSymlink(string path)
    {
        if (!SymlinkHelper.IsSymlink(path))
            throw new IOException($"Not a symbolic link: {path}");
    }
    private static void RunCommand(string fileName, string args)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };

        process.Start();
        string stderr = process.StandardError.ReadToEnd();
        string stdout = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
            throw new Exception($"Command failed: {fileName} {args}\nstdout: {stdout}\nstderr: {stderr}");
    }
}