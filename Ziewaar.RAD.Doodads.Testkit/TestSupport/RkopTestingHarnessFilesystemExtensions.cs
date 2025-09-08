namespace Ziewaar.RAD.Doodads.Testkit;
public static class RkopTestingHarnessFilesystemExtensions
{
    public static string RelativeFile(this RkopTestingHarness harness, string relativeName)
    {
        return Path.Combine(harness.WorkingDirectory, relativeName);
    }
    public static RkopTestingHarness VerbatimFile(this RkopTestingHarness harness, string relativeName, string content)
    {
        File.WriteAllText(harness.RelativeFile(relativeName), content);
        return harness;
    }
    public static RkopTestingHarness BinFile(this RkopTestingHarness harness, string relativeName)
    {
        var fullAssemblyPath = typeof(RkopTestingHarnessFilesystemExtensions).Assembly.Location;
        var assemblyDirectory = Path.GetDirectoryName(fullAssemblyPath);
        if (string.IsNullOrWhiteSpace(assemblyDirectory) || !Directory.Exists(assemblyDirectory))
            throw new IOException("Dirless Assembly");
        var fileInOutput = Path.Combine(assemblyDirectory, relativeName);
        if (!File.Exists(fileInOutput))
            throw new IOException("File has not been found in output dir");
        File.Copy(fileInOutput, harness.RelativeFile(relativeName));
        return harness;
    }
}