using System.Data;
using System.Reflection.Metadata;
using Serilog;
using Ziewaar.RAD.Doodads.CommonComponents;
using Ziewaar.RAD.Doodads.CommonComponents.TextTemplating;
using Ziewaar.RAD.Doodads.CoreLibrary;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.Cryptography;
using Ziewaar.RAD.Doodads.Cryptography.Secrets;
using Ziewaar.RAD.Doodads.FormsValidation.Services;
using Ziewaar.RAD.Doodads.FormsValidation.Services.UrlEncodedOnly;
using Ziewaar.RAD.Doodads.ModuleLoader.Services;
using Ziewaar.RAD.Doodads.MySQL;
using Ziewaar.RAD.Doodads.SQLite;
using Ziewaar.RAD.Doodads.StandaloneWebserver.Services;
using Ziewaar.RAD.Starter;
using DataRow = Ziewaar.RAD.Doodads.Data.Services.DataRow;

namespace Ziewaar.RAD.Doodads.RuntimeForDotnetCore
{
#nullable enable
    public class Program
    {
        private static void Main(string[] args)
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

            var myDir = Path.GetDirectoryName(typeof(Program).Assembly.Location);
            
            BootstrappedStartBuilder
                .Create(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "doodads"))
                .AddAssemblyBy<IService>().AddAssemblyBy<WebServer>().AddAssemblyBy<Template>()
                .AddAssemblyBy<Definition>().AddAssemblyBy<SqliteConnectionSource>().AddAssemblyBy<HtmlForm>()
                .AddAssemblyBy<LoadSensitive>().AddAssemblyBy<DataRow>()
                .AddAssemblyBy<MySqlConnectionSource>().
                AddFile("site.rkop", myDir != null ? File.ReadAllText(Path.Combine(myDir, "site.rkop")) : "").
                AddFile("server.rkop",  myDir != null ? File.ReadAllText(Path.Combine(myDir, "server.rkop")) : "").
                AddFile("boot.rkop", myDir != null ? File.ReadAllText(Path.Combine(myDir, "boot.rkop")) : "").
                SetStarter("boot.rkop").ReadArgs(args).Build().Run();
        }
    }
}