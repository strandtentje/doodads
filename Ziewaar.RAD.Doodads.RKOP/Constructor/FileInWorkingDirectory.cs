namespace Ziewaar.RAD.Doodads.RKOP.Constructor;

public record struct FileInWorkingDirectory(char prefix, string WorkingDirectory, string RelativePath)
{
    public static implicit operator (string WorkingDirectory, string RelativePath)(FileInWorkingDirectory value)
    {
        return (value.WorkingDirectory, value.RelativePath);
    }
    public static implicit operator FileInWorkingDirectory((char prefix, string WorkingDirectory, string RelativePath) value)
    {
        return new FileInWorkingDirectory(value.prefix, value.WorkingDirectory, value.RelativePath);
    }
    public override string ToString() => Path.Combine(WorkingDirectory, RelativePath);
}