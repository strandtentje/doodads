namespace Ziewaar.RAD.Doodads.ModuleLoader.Services.Reflection;
public class FullNameComparer : IEqualityComparer<(string FullName, DirectoryInfo DirectoryInfo)>
{
    public bool Equals((string FullName, DirectoryInfo DirectoryInfo) x, (string FullName, DirectoryInfo DirectoryInfo) y) => x.FullName == y.FullName;
    public int GetHashCode((string FullName, DirectoryInfo DirectoryInfo) obj) => obj.FullName.GetHashCode();
}