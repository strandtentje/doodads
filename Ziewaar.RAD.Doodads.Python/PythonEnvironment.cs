namespace Ziewaar.RAD.Doodads.Python;

public class PythonEnvironment : IService, IDisposable
{
    private readonly SingletonResourceRepository<PythonEnvironmentParameters, ServiceProvider> Environments =
        SingletonResourceRepository<PythonEnvironmentParameters, ServiceProvider>.Get();

    private readonly UpdatingKeyValue WorkingDirectoryConstant = new("home");
    private readonly UpdatingKeyValue VenvDirectoryConstant = new("venv");
    private readonly UpdatingKeyValue VersionNumberConstant = new("version");
    private readonly UpdatingKeyValue RequirementsFileConstant = new("requirements");
    private PythonEnvironmentParameters CurrentEnvironmentParameters = new(), NewEnvironmentParameters = new();
    private Guid CurrentEnvironmentGuid = Guid.Empty;
    private ServiceProvider? CurrentEnvironment;


    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
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
            OnException?.Invoke(this, new CommonInteraction(interaction, "home, venv, requirements and version are required."));
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
        foreach (var p in arr[1].AsEnumerable<PyObject>()) GlobalLog.Instance?.Information("Python PATH: " + p.As<string>());

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