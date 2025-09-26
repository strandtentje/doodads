using Serilog;
using Ziewaar.RAD.Doodads.CommonComponents.TextTemplating;
using Ziewaar.RAD.Doodads.CoreLibrary;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.Cryptography;
using Ziewaar.RAD.Doodads.Cryptography.Secrets;
using Ziewaar.RAD.Doodads.Data.Services;
using Ziewaar.RAD.Doodads.FormsValidation.Services.UrlEncodedOnly;
using Ziewaar.RAD.Doodads.ModuleLoader.Services;
using Ziewaar.RAD.Doodads.MySQL;
using Ziewaar.RAD.Doodads.SQLite;
using Ziewaar.RAD.Doodads.StandaloneWebserver.Services;
using Ziewaar.RAD.Starter;

namespace Ziewaar.RAD.Doodads.Testkit;
public class RkopTestingHarness : IDisposable
{
    private object RunLock = new object();
    private readonly BootstrappedStart Starter;
    public string WorkingDirectory => Starter.WorkingDirectory;
    public RkopTestingHarness(string rkopText)
    {
        var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "doodads",
            "logging");
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        string logfilePath = Path.Combine(dir, $"{DateTime.Now:yyMMddHHmm}.log.txt");
        GlobalLog.Instance = new LoggerConfiguration().WriteTo.File(
                logfilePath
            ).WriteTo.Console().
#if DEBUG
            MinimumLevel.Debug().
#endif
            CreateLogger();
        GlobalLog.Instance.Information("Logfile in: {file}", logfilePath);
                
        var generalTempDir = Path.GetTempPath();
        var tempSubDir = Path.GetRandomFileName();
        var workingDirectory = Path.Combine(generalTempDir, tempSubDir);
        this.Starter = BootstrappedStartBuilder
            .Create(workingDirectory)
            .AddAssemblyBy<IService>().AddAssemblyBy<WebServer>().AddAssemblyBy<Template>()
            .AddAssemblyBy<Definition>().AddAssemblyBy<SqliteConnectionSource>().AddAssemblyBy<HtmlForm>()
            .AddAssemblyBy<LoadSensitive>().AddAssemblyBy<DataRow>()
            .AddAssemblyBy<MySqlConnectionSource>().AddFile("test.rkop", rkopText)
            .SetStarter("test.rkop").Build();
    }
    public RkopTestingHarness Run(IInteraction interaction)
    {
        this.Starter.Run(new TestingOffsetInteraction(interaction));
        return this;
    }
    public void Dispose()
    {
        Directory.Delete(this.Starter.WorkingDirectory, true);
    }
}