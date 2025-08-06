using System.Data;
using System.Reflection.Metadata;
using Serilog;
using Ziewaar.RAD.Doodads.CommonComponents;
using Ziewaar.RAD.Doodads.CommonComponents.TextTemplating;
using Ziewaar.RAD.Doodads.CoreLibrary;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.Cryptography;
using Ziewaar.RAD.Doodads.FormsValidation.Services;
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
                MinimumLevel.Verbose().
#endif
                CreateLogger();
            GlobalLog.Instance.Information("Logfile in: {file}", logfilePath);

            BootstrappedStartBuilder
                .Create(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "doodads"))
                .AddAssemblyBy<IService>().AddAssemblyBy<WebServer>().AddAssemblyBy<Template>()
                .AddAssemblyBy<Definition>().AddAssemblyBy<SqliteConnectionSource>().AddAssemblyBy<ValidateForm>()
                .AddAssemblyBy<LoadSensitive>().AddAssemblyBy<DataRow>()
                .AddAssemblyBy<MySqlConnectionSource>().
                AddFile("site.rkop", """
                <<>> {
                    : Route("GET /static") : Fileserver(f"static")
                    | ExactRoute("GET /favicon.ico") : HttpStatus(404)
                    | ExactRoute("GET /") : Print("
                        <html><head><title>Doodads!</title></head>
                        <body>
                            <h1>Doodads!</h1>
                            <p>This is the default page.</p>
                        </body></html>", 
                        contenttype="text/html")
                    | HttpStatus(404) : Print("
                        <html><head><title>Page not found</title></head>
                        <body>
                            <h1>Doodads!</h1>
                            <p>There's nothing here.</p>
                        </body></html>", 
                        contenttype="text/html");
                };
                """
                ).
                AddFile("server.rkop",
                """
                <<>> : WebServer(["http://+:8243/"]) {
                    : <f"site.rkop">;
                    OnStarted->Dump("Webserver Running", limit = 0) & [:];
                    OnHead->Template():$"[{% requesttime %}|{% remoteip %}] {% <method %} {% <url %}";
                    OnStopping->$"Webserver Stopped";
                    OnException->!"message" : ConsoleOutput() : $"Server complains {% message %}";
                };
                """).
                AddFile("boot.rkop",
                """
                <<>> 
                    : Hold("Lock against shutting down")
                    : ConsoleOutput() : StartWebServer() : <f"server.rkop">
                    : $"Doodads. Type help for commands." : ConsoleInput() : Open("Console Input for Reading Commands")
                    :! "Command Reader" : Repeat("To stay interactive") : $"#" : Pop("Command Reader") {
                        : $"... "
                        & ~"exit":Close("Console Input for Reading Commands"):Release("Lock against shutting down")
                        | ~"ver":$"version -1":Continue("To stay interactive")
                        | ~"stop":StopWebServer():<f"server.rkop">:Continue("To stay interactive")
                        | ~"start":StartWebServer():<f"server.rkop">:Continue("To stay interactive")
                        | ~"config" : ShellExecute() { |$f""; } : $"Opened config directory"  : Continue("To stay interactive")
                        | ~"appdata": ShellExecute() { |$c""; } : $"Opened appdata directory" : Continue("To stay interactive")
                        | ~"profile": ShellExecute() { |$p""; } : $"Opened home directory"    : Continue("To stay interactive")
                        | ~"browse" : ShellExecute() { |$"{% localipurl %}"; } : $"Opened {% localipurl %} in browser": Continue("To stay interactive")
                        | ~"help":$"
                        exit: stop runtime
                        ver:  version information
                        stop: stop server
                        start: start server
                        browse: open in browser
                        config: open configuration directory
                        appdata: open appdata directory
                        profile: open user profile directory"
                        : Continue("To stay interactive")
                        | Continue("To stay interactive");
                    };    
                """).SetStarter("boot.rkop").ReadArgs(args).Build().Run();
        }
    }
}