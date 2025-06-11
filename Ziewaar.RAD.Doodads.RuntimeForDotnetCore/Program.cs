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
    private static void Main(string[] args)
    {
        var parsedArgs = ArgParser.Parse(args);

        var files = parsedArgs.Filenames;
        if (!files.Any())
        {
            files.Add(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
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
            foreach (var item in files)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(item) ?? throw new Exception("Invalid directory."));

                if (!File.Exists(item))
                {
                    using (var cfg = File.CreateText(item))
                    {
                        cfg.Write("""
    Definition():Hold("Lock against shutting down") {
      OnThen->ConsoleOutput()
            : Print("Doodads. Type help for commands.")
            : ConsoleInput()
            : Open("Console Input for Reading Commands")
            : Store("Command Reader")
            : Repeat("To stay interactive")
            : Pop("Command Reader") {
                OnThen->
                      Case("exit"):Close("Console Input for Reading Commands"):Release("Lock against shutting down")
                    | Case("ver"):Print("version -1"):Continue("To stay interactive")
                    | Print("exit: stop runtime"):Print("ver:  version information"):Continue("To stay interactive");
      };    
    }; 
    """);
                    }
                }

                ProgramRepository.Instance.GetForFile(item, autoStartOnReloadParams: rootInteraction);
            }
        }
        finally
        {
            FileWatcherFactory.Instance.Dispose();
        }
    }
}