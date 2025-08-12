namespace Ziewaar.RAD.Doodads.Python;
public record struct PythonEnvironmentParameters : IComparable
{
    public string? WorkingDirectory, VenvDirectory, VersionString;

    public bool TryValidate(out string workingDirectory, out string venvDirectory, out string versionString)
    {
        if (!string.IsNullOrWhiteSpace(WorkingDirectory) &&
            !string.IsNullOrWhiteSpace(VenvDirectory) &&
            !string.IsNullOrWhiteSpace(VersionString))
        {
            workingDirectory = WorkingDirectory;
            venvDirectory = VenvDirectory;
            versionString = VersionString;
            return true;
        }
        else
        {
            workingDirectory = "";
            venvDirectory = "";
            versionString = "";
            return false;
        }
    }

    public int CompareTo(object? obj)
    {
        if (obj is not PythonEnvironmentParameters other)
            throw new ArgumentException("Object is not a PythonEnvironmentParameters");
        return (WorkingDirectory, VenvDirectory, VersionString).CompareTo((other.WorkingDirectory, other.VenvDirectory,
            other.VersionString));
    }
}