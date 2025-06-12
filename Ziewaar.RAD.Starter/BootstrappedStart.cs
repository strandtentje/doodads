#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;
using Ziewaar.RAD.Doodads.ModuleLoader;
using File = System.IO.File;
namespace Ziewaar.RAD.Starter;
public class BootstrappedStart(string workingDirectory, Assembly[] populateAssemblies, string[] loadFiles, string startFile, SortedList<string, object> rootInteractionMemory)
{
    public string WorkingDirectory => workingDirectory;
    public Assembly[] PopulateAssemblies => populateAssemblies;
    public IReadOnlyList<string> LoadFiles => loadFiles;
    public string StartFile => startFile;
    public IReadOnlyDictionary<string, object> RootInteractionMemory => rootInteractionMemory;
    public void Run()
    {
        Environment.CurrentDirectory = WorkingDirectory;
        foreach (var item in populateAssemblies)
            TypeRepository.Instance.PopulateWith(item);
        var rootInteraction = new RootInteraction("", rootInteractionMemory);
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
