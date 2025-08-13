namespace Ziewaar.RAD.Doodads.Python;

public record struct PythonEnvironmentParameters : IComparable
{
    public string? WorkingDirectory, VenvDirectory, VersionString, RequirementsFile;

    public bool TryValidate(out string workingDirectory, out string venvDirectory, out string versionString,
        out string requirementsFile)
    {
        if (!string.IsNullOrWhiteSpace(WorkingDirectory) &&
            !string.IsNullOrWhiteSpace(VenvDirectory) &&
            !string.IsNullOrWhiteSpace(VersionString) &&
            !string.IsNullOrWhiteSpace(RequirementsFile))
        {
            workingDirectory = WorkingDirectory;
            venvDirectory = VenvDirectory;
            versionString = VersionString;
            requirementsFile = RequirementsFile;
            return true;
        }
        else
        {
            workingDirectory = "";
            venvDirectory = "";
            versionString = "";
            requirementsFile = "";
            return false;
        }
    }

    public int CompareTo(object? obj)
    {
        if (obj is not PythonEnvironmentParameters other)
            throw new ArgumentException("Object is not a PythonEnvironmentParameters");
        return (WorkingDirectory, VenvDirectory, VersionString, RequirementsFile).CompareTo((other.WorkingDirectory,
            other.VenvDirectory,
            other.VersionString, other.RequirementsFile));
    }
}