namespace Ziewaar.RAD.Doodads.Python;
public class PythonEnvironment : IService, IDisposable
{
    private readonly SingletonResourceRepository<PythonEnvironmentParameters, ServiceProvider> Environments =
        SingletonResourceRepository<PythonEnvironmentParameters, ServiceProvider>.Get();

    private readonly UpdatingKeyValue WorkingDirectoryConstant = new("home");
    private readonly UpdatingKeyValue VenvDirectoryConstant = new("venv");
    private readonly UpdatingKeyValue VersionNumberConstant = new("version");
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

        if (!this.NewEnvironmentParameters.Equals(this.CurrentEnvironmentParameters) &&
            !this.NewEnvironmentParameters.TryValidate(out string wd, out string venv, out string ver))
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "home, venv and version are required."));
            return;
        }
        else if (!this.NewEnvironmentParameters.Equals(this.CurrentEnvironmentParameters))
        {
            if (this.CurrentEnvironmentGuid != Guid.Empty)
                Environments.Return(this.CurrentEnvironmentParameters, this.CurrentEnvironmentGuid);
            this.CurrentEnvironmentParameters = this.NewEnvironmentParameters;
            (this.CurrentEnvironmentGuid, this.CurrentEnvironment) =
                Environments.Take(this.CurrentEnvironmentParameters, EnvironmentFactory);
        }

        if (this.CurrentEnvironment == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "environment not setup"));
            return;
        }

        IPythonEnvironment environment = this.CurrentEnvironment.GetRequiredService<IPythonEnvironment>();
        OnThen?.Invoke(this, new PythonEnvironmentInteraction(
            interaction, environment));
    }

    private ServiceProvider EnvironmentFactory(PythonEnvironmentParameters arg)
    {
        return new ServiceCollection().WithPython().WithHome(arg.WorkingDirectory!)
            .WithVirtualEnvironment(arg.VenvDirectory!).WithPipInstaller()
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