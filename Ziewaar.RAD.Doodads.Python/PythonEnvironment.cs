using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;

namespace Ziewaar.RAD.Doodads.Python;

[Category("Interop & Languages")]
[Title("Setup a Python venv")]
[Description("""
             Provided a working directory, venv directory, python version and requirements file,
             will set up and prepare a python environmen to use under OnThen
             """)]
public class PythonEnvironment : IService, IDisposable
{
    private readonly SingletonResourceRepository<PythonEnvironmentParameters, ServiceProvider> Environments =
        SingletonResourceRepository<PythonEnvironmentParameters, ServiceProvider>.Get();

    [NamedSetting("home", "Path to working directory")]
    private readonly UpdatingKeyValue WorkingDirectoryConstant = new("home");

    [NamedSetting("venv", "Venv path")] private readonly UpdatingKeyValue VenvDirectoryConstant = new("venv");

    [NamedSetting("version", "Python version")]
    private readonly UpdatingKeyValue VersionNumberConstant = new("version");

    [NamedSetting("requirements", "Path to requirements.txt file")]
    private readonly UpdatingKeyValue RequirementsFileConstant = new("requirements");

    private PythonEnvironmentParameters CurrentEnvironmentParameters = new(), NewEnvironmentParameters = new();
    private Guid CurrentEnvironmentGuid = Guid.Empty;
    private ServiceProvider? CurrentEnvironment;

    [EventOccasion("Python environment is available here.")]
    public event CallForInteraction? OnThen;

    [NeverHappens] public event CallForInteraction? OnElse;

    [EventOccasion("""
                   Likely happens when one of the paths was incorrect or missing, 
                   or the venv couldnt be set up.
                   """)]
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, WorkingDirectoryConstant).IsRereadRequired(out object? workingDirectoryCandidate))
            this.NewEnvironmentParameters.WorkingDirectory = workingDirectoryCandidate?.ToString();
        if ((constants, VenvDirectoryConstant).IsRereadRequired(out object? venvDirectoryCandidate))
            this.NewEnvironmentParameters.VenvDirectory = venvDirectoryCandidate?.ToString();
        if ((constants, VersionNumberConstant).IsRereadRequired(out object? versionCandidate))
            this.NewEnvironmentParameters.VersionString = versionCandidate?.ToString();
        if ((constants, RequirementsFileConstant).IsRereadRequired(out object? requirementsCandidate))
            this.NewEnvironmentParameters.RequirementsFile = requirementsCandidate?.ToString();

        if (!this.NewEnvironmentParameters.Equals(this.CurrentEnvironmentParameters) &&
            !this.NewEnvironmentParameters.TryValidate(out string wd, out string venv, out string ver, out string req))
        {
            OnException?.Invoke(this,
                new CommonInteraction(interaction, "home, venv, requirements and version are required."));
            return;
        }
        else if (!this.NewEnvironmentParameters.Equals(this.CurrentEnvironmentParameters))
        {
            if (this.CurrentEnvironmentGuid != Guid.Empty)
                Environments.Return(this.CurrentEnvironmentParameters, this.CurrentEnvironmentGuid);
            this.CurrentEnvironmentParameters = this.NewEnvironmentParameters;
            GlobalLog.Instance?.Information(
                "No python environment or environment expired; disposing and rebuilding...");
            (this.CurrentEnvironmentGuid, this.CurrentEnvironment) =
                Environments.Take(this.CurrentEnvironmentParameters, EnvironmentFactory);
            GlobalLog.Instance?.Information("Rebuilt.");
        }

        if (this.CurrentEnvironment == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "environment not setup"));
            return;
        }

        IPythonEnvironment environment = this.CurrentEnvironment.GetRequiredService<IPythonEnvironment>();
        GlobalLog.Instance?.Information("Acquired python {version} environment", environment.Version);

        // Current working dir and module search paths
        var paths = environment.ExecuteExpression("(__import__('os').getcwd(), list(__import__('sys').path))");
        var arr = paths.AsEnumerable<PyObject>().ToArray();
        GlobalLog.Instance?.Information("Python wording dir {wd}", arr[0].As<string>());
        foreach (var p in arr[1].AsEnumerable<PyObject>())
            GlobalLog.Instance?.Information("Python PATH: " + p.As<string>());

        OnThen?.Invoke(this, new PythonEnvironmentInteraction(
            interaction, environment));
    }

    private ServiceProvider EnvironmentFactory(PythonEnvironmentParameters arg)
    {
        return new ServiceCollection().WithPython().WithHome(arg.WorkingDirectory!)
            .WithVirtualEnvironment(arg.VenvDirectory!).WithPipInstaller(arg.RequirementsFile!)
            .FromNuGet(arg.VersionString!)
            .FromRedistributable(arg.VersionString!)
            .FromMacOSInstallerLocator(arg.VersionString!)
            .FromWindowsInstaller(arg.VersionString!)
            .Services.BuildServiceProvider();
    }

    public void HandleFatal(IInteraction source, Exception ex) =>
        OnException?.Invoke(this, source);

    public void Dispose()
    {
        if (this.CurrentEnvironmentGuid != Guid.Empty)
            Environments.Return(this.CurrentEnvironmentParameters, this.CurrentEnvironmentGuid);
    }
}