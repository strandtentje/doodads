#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Ziewaar.Common.Aardvargs;

namespace Ziewaar.RAD.Starter;

public class BootstrappedStartBuilder
{
    private string _workingDirectory;
    public string WorkingDirectory => _workingDirectory;
    private readonly List<(string name, string definition)> _fileDefinitions = new();
    private readonly HashSet<Assembly> _assembliesToLoad = new();
    private SortedList<string, object> _rootContext = new();
    private string? StarterFile;
    public IReadOnlyList<(string name, string definition)> FileDefinitions => _fileDefinitions;
    public IEnumerable<Assembly> AssembliesToLoad => _assembliesToLoad;
    public IReadOnlyDictionary<string, object> RootContext => _rootContext;
    private BootstrappedStartBuilder(string? workingDirectory) => this._workingDirectory = workingDirectory ?? Environment.CurrentDirectory;
    public static BootstrappedStartBuilder Create(string workingDirectory) => new BootstrappedStartBuilder("".QualifyFullPath(workingDirectory));
    public BootstrappedStartBuilder ReadArgs(string[] args)
    {
        var x = ArgParser.Parse(args);
        var workingDirectory = x.Options.GetValuesStartingWith<string>("w").SingleOrDefault();
        var bootFiles = x.Options.GetValuesStartingWith<string>("b");
        if (bootFiles.Length == 0 && x.Filenames.Count > 0)
            bootFiles = [.. x.Filenames];
        var starterFile =
            x.Options.GetValuesStartingWith<string>("s").SingleOrDefault()
            ?? bootFiles.LastOrDefault();

        _workingDirectory = workingDirectory ?? _workingDirectory;        
        if (bootFiles.Any())
        {
            _fileDefinitions.Clear();
            foreach (var item in bootFiles)
                AddFile(item);
        }
        if (starterFile != null)
        {
            this.SetStarter(starterFile);
        }
        var rootContext = x.Options.RemoveValuesStartingWithAny("w", "b", "s");
        if (rootContext.Any())
            this.SetRootContext(rootContext);
        return this;
    }
    public BootstrappedStartBuilder SetRootContext(SortedList<string, object> sortedList)
    {
        this._rootContext = sortedList;
        return this;
    }
    public BootstrappedStartBuilder AddFile(string name, string? definitionText = null)
    {
        Directory.CreateDirectory(_workingDirectory);
        string fullPath = name.QualifyFullPath(_workingDirectory);
        var rkopFile = new FileInfo(fullPath);
        if (!rkopFile.Exists)
        {
            if (definitionText != null)
            {
                var dirpath = Path.GetDirectoryName(fullPath);
                if (!Directory.Exists(dirpath))
                    Directory.CreateDirectory(dirpath);
                using (var writer = rkopFile.CreateText())
                {
                    writer.Write(definitionText);
                }
            }
            else
            {
                throw new ArgumentException("Add an existing file or specify default definition text.", nameof(definitionText));
            }
        }
        else
        {
            using (var reader = rkopFile.OpenText())
            {
                definitionText = reader.ReadToEnd();
            }
        }
        _fileDefinitions.Add((name.TruncatePathStart(_workingDirectory), definitionText));
        return this;
    }
    public BootstrappedStartBuilder SetStarter(string name)
    {
        string truncatedName = name.TruncatePathStart(_workingDirectory);
        if (!FileDefinitions.Any(x => x.name == truncatedName))
            throw new ArgumentException("starter file should have been added prior", nameof(name));
        this.StarterFile = name;
        return this;
    }
    public string TrimmedWorkingDirectory => _workingDirectory.TrimEnd(Path.DirectorySeparatorChar);
    public BootstrappedStartBuilder AddAssembly(Assembly assembly)
    {
        _assembliesToLoad.Add(assembly);    
        return this;
    }
    public BootstrappedStartBuilder AddAssemblyBy<TType>() => AddAssembly(typeof(TType).Assembly);
    public string[] BuildArgs()
    {
        var workingDirectoryFlags = _workingDirectory.ToFilenameFlags("w");        
        var bootDefFlags = FileDefinitions.ToMultipleFilenameFlags("b", x => x.name);
        var starterFileFlags = (StarterFile ?? FileDefinitions.Last().name).ToFilenameFlags("s");
        return
        [
            .. workingDirectoryFlags,
            "",
            .. bootDefFlags,
            .. starterFileFlags,
        ];
    }
    private string[] RequireFullyQualifiedDefinitionFilePaths() =>
        FileDefinitions.Select(x => x.name.QualifyFullPath(WorkingDirectory)).ToArray();
    private string RequireFullyQualifiedStarterFilePath() =>
        (StarterFile ?? FileDefinitions.LastOrDefault().name ??
            throw new ArgumentException("start file required", nameof(StarterFile))).
        QualifyFullPath(WorkingDirectory);
    public BootstrappedStart Build() => new BootstrappedStart(
        workingDirectory: _workingDirectory,
        populateAssemblies: _assembliesToLoad.ToArray(),
        loadFiles: RequireFullyQualifiedDefinitionFilePaths(),
        startFile: RequireFullyQualifiedStarterFilePath(),
        rootInteractionMemory: _rootContext
        );
}
