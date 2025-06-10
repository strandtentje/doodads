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

        foreach (var item in files)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(item) ?? throw new Exception("Invalid directory."));

            if (!File.Exists(item))
            {
                using (var cfg = File.CreateText(item))
                {
                    cfg.Write("""
                        Definition():Hold("boot") {
                            OnThen->ConsoleOutput():ConstantTextSource("Doodads") 
                                  & StartLineReader():Repeat() {
                                      OnThen->First():Option("exit") {
                                        OnThen->StopLineReader():Release("boot");
                                        OnElse->Continue();
                                      };
                                  };
                        };
                        """);
                }
            }

            var program = ProgramRepository.Instance.GetForFile(item);
            program.EntryPoint.Run(new object(), rootInteraction);
        }
    }
}