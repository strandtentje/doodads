using Ziewaar.RAD.Doodads.CommonComponents;
using Ziewaar.RAD.Doodads.CommonComponents.TextTemplating;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.FormsValidation.Services;
using Ziewaar.RAD.Doodads.ModuleLoader.Services;
using Ziewaar.RAD.Doodads.MySQL;
using Ziewaar.RAD.Doodads.SQLite;
using Ziewaar.RAD.Doodads.StandaloneWebserver.Services;
using Ziewaar.RAD.Starter;

namespace Ziewaar.RAD.Doodads.RuntimeForDotnetCore
{
#nullable enable
    public class Program
    {
        private static void Main(string[] args) =>
            BootstrappedStartBuilder.
            Create(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "doodads")).
            AddAssemblyBy<IService>().
            AddAssemblyBy<WebServer>().
            AddAssemblyBy<Template>().
            AddAssemblyBy<Definition>().
            AddAssemblyBy<SqliteConnectionSource>().
            AddAssemblyBy<ValidateForm>().
            AddAssemblyBy<MySqlConnectionSource>().
            AddFile("server.rkop",
            """
Definition()
{
    OnThen->WebServer(["http://localhost:8243/"]) {
        OnStarted->Print("server started");
        OnHead->Template():Print("[{% requesttime %}|{% remoteip %}] {% <method %} {% <url %}");
        OnThen->Load("url"):Case("/"):Print("Doodads!");
        OnStopping->Print("cleaning up after server");
        OnException->Store("message"):ConsoleOutput():Template():Print("Server complains {% message %}");
    } & ReturnThen();
}            
""").
            AddFile("boot.rkop",
            """
Definition():Hold("Lock against shutting down"):ConsoleOutput() {
    OnThen->StartWebServer()
        : Call(f"server.rkop")
        : Print("Doodads. Type help for commands.")
        : ConsoleInput()
        : Open("Console Input for Reading Commands")
        : Store("Command Reader")
        : Repeat("To stay interactive")
        : Print("#")
        : Pop("Command Reader") {
            OnThen->
                    Print("...")
                & Case("exit"):Close("Console Input for Reading Commands"):Release("Lock against shutting down")
                | Case("ver"):Print("version -1"):Continue("To stay interactive")
                | Case("stop"):StopWebServer():Call(f"server.rkop"):Continue("To stay interactive")
                | Case("start"):StartWebServer():Call(f"server.rkop"):Continue("To stay interactive")
                | Print("exit: stop runtime")
                : Print("ver:  version information")
                : Print("stop: stop server")
                : Print("start: start server")
                : Continue("To stay interactive");
    };    
}; 
""").
            SetStarter("boot.rkop").
            ReadArgs(args).
            Build().
            Run();
    }
}