#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Ziewaar.RAD.Doodads.CoreLibrary;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;
using Ziewaar.RAD.Doodads.ModuleLoader;
using Ziewaar.RAD.Doodads.ModuleLoader.Filesystem;
using File = System.IO.File;
namespace Ziewaar.RAD.Starter;
public class BootstrappedStart(string workingDirectory, Assembly[] populateAssemblies, string[] loadFiles, string startFile, SortedList<string, object> rootInteractionMemory)
{
    public string WorkingDirectory => workingDirectory;
    [JsonIgnore]
    public Assembly[] PopulateAssemblies => populateAssemblies;
    public IEnumerable<string> AssemblyNames => PopulateAssemblies.Select(x => x.FullName);
    public IReadOnlyList<string> LoadFiles => loadFiles;
    public string StartFile => startFile;
    public IReadOnlyDictionary<string, object> RootInteractionMemory => rootInteractionMemory;
    public void Run(IInteraction? rootInteraction = null)
    {
        GlobalLog.Instance?.Information("Bootstrapped start information: {info}", JsonConvert.SerializeObject(this, Formatting.Indented));
        Environment.CurrentDirectory = WorkingDirectory;
        foreach (var item in populateAssemblies)
            TypeRepository.Instance.PopulateWith(item);
        rootInteraction ??= new RootInteraction("", rootInteractionMemory);
        try
        {
            foreach (var item in loadFiles)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(item) ?? throw new Exception("Invalid directory."));

                if (!File.Exists(item))
                    File.Create(item).Close();

                if (item == startFile)
                    ProgramRepository.Instance.GetForFile(item, rootInteraction);
                else
                    ProgramRepository.Instance.GetForFile(item);
            }
        }
        finally
        {
            FileWatcherFactory.Instance.Dispose();
        }
    }
}
