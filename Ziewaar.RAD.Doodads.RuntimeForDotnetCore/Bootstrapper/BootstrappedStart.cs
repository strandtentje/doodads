using System.Reflection;
using Newtonsoft.Json;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;
using Ziewaar.RAD.Doodads.ModuleLoader;
using Ziewaar.RAD.Doodads.ModuleLoader.Filesystem;
using File = System.IO.File;

namespace Ziewaar.RAD.Doodads.RuntimeForDotnetCore.Bootstrapper;

public class BootstrappedStart(
    string workingDirectory,
    Assembly[] populateAssemblies,
    string[] loadFiles,
    string startFile,
    SortedList<string, object> rootInteractionMemory)
{
    private string WorkingDirectory => workingDirectory;
    [JsonIgnore] private Assembly[] PopulateAssemblies => populateAssemblies;

    public IEnumerable<string> AssemblyNames =>
        PopulateAssemblies.Select(x => x.FullName).OfType<string>();

    public IReadOnlyList<string> LoadFiles => loadFiles;
    public string StartFile => startFile;
    public IReadOnlyDictionary<string, object> RootInteractionMemory => rootInteractionMemory;

    public IDisposable Run(IInteraction? rootInteraction = null, Func<IInteraction, IInteraction>? interactionInjection = null)
    {
        // GlobalLog.Instance?.Information("Bootstrapped start information: {info}", JsonConvert.SerializeObject(this, Formatting.Indented));
        Environment.CurrentDirectory = WorkingDirectory;
        foreach (var item in populateAssemblies)
            TypeRepository.Instance.PopulateWith(item);
        rootInteraction ??= new RootInteraction("", rootInteractionMemory);
        if (interactionInjection != null)
            rootInteraction = interactionInjection(rootInteraction);
        var multipleDisposables = new MultipleDisposables();
        try
        {
            foreach (var item in loadFiles)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(item) ??
                                          throw new Exception("Invalid directory."));

                if (!File.Exists(item))
                    File.Create(item).Close();

                if (item == startFile)
                    multipleDisposables.Add(ProgramRepository.Instance.GetForFile(item, rootInteraction));
                else
                    multipleDisposables.Add(ProgramRepository.Instance.GetForFile(item));
            }
        }
        finally
        {
            FileWatcherFactory.Instance.Dispose();
        }
        return multipleDisposables;
    }
}
