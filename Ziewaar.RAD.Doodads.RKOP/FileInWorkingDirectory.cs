namespace Ziewaar.RAD.Doodads.RKOP;

public record struct FileInWorkingDirectory(string WorkingDirectory, string RelativePath)
{
    public static implicit operator (string WorkingDirectory, string RelativePath)(FileInWorkingDirectory value)
    {
        return (value.WorkingDirectory, value.RelativePath);
    }
    public static implicit operator FileInWorkingDirectory((string WorkingDirectory, string RelativePath) value)
    {
        return new FileInWorkingDirectory(value.WorkingDirectory, value.RelativePath);
    }
    public override string ToString() => Path.Combine(WorkingDirectory, RelativePath);
}