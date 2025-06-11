using Ziewaar.Common.Aardvargs;
using Ziewaar.RAD.Doodads.CommonComponents;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;
using Ziewaar.RAD.Doodads.ModuleLoader;
using Ziewaar.RAD.Doodads.ModuleLoader.Services;
using Ziewaar.RAD.Doodads.StandaloneWebserver.Services;
#nullable enable
internal class Program
{
    private static readonly SortedList<string, string> cleanFiles = new()
    {
        {
            "boot.rkop",
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
            """
        },
        {
            "server.rkop",
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
            """
        }
    };

    private static void Main(string[] args)
    {
        var parsedArgs = ArgParser.Parse(args);

        var files = parsedArgs.Filenames;
        if (!files.Any())
        {
            string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            files.Add(Path.Combine(
                appdata,
                "Doodads",
                "server.rkop"));
            files.Add(Path.Combine(
                appdata,
                "Doodads",
                "boot.rkop"));
        }

        TypeRepository.Instance.
            PopulateWith(typeof(IService).Assembly).
            PopulateWith(typeof(WebServer).Assembly).
            PopulateWith(typeof(Template).Assembly).
            PopulateWith(typeof(Definition).Assembly);

        var rootInteraction = new RootInteraction("", parsedArgs.Options);

        try
        {
            KnownProgram? lastOne = null;
            foreach (var item in files)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(item) ?? throw new Exception("Invalid directory."));

                if (!File.Exists(item))
                {
                    using (var cfg = File.CreateText(item))
                    {
                        cfg.Write(cleanFiles[Path.GetFileName(item)]);
                    }
                }

                lastOne = ProgramRepository.Instance.GetForFile(item);
            }
            if (lastOne != null && lastOne.EntryPoint != null)
                lastOne.EntryPoint.Run(new object(), rootInteraction);
            else
                Console.Write("no entry point");
        }
        finally
        {
            FileWatcherFactory.Instance.Dispose();
        }
    }
}