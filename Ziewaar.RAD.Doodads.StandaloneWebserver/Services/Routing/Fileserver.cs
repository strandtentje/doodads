using System.Collections;
using Ziewaar.RAD.Doodads.CommonComponents.LiteralSource;
using Ziewaar.RAD.Doodads.ModuleLoader.Services;
using Ziewaar.RAD.Doodads.RKOP.Constructor;
using Ziewaar.RAD.Doodads.StandaloneWebserver.Services.Routing;

namespace Ziewaar.RAD.Doodads.CommonComponents.IO
{
    public class Fileserver : IService
    {
        private readonly UpdatingPrimaryValue DirectoryToServeConst = new UpdatingPrimaryValue();
        private readonly UpdatingKeyValue DefaultIndexFilesConst = new UpdatingKeyValue("indexfiles");
        private readonly UpdatingKeyValue RunProgramsConst = new UpdatingKeyValue("run");

        private string? DirectoryToServe;
        private string[] DefaultIndexFiles = [];
        private bool RunPrograms;

        public event CallForInteraction? OnThen;
        public event CallForInteraction? OnElse;
        public event CallForInteraction? OnException;

        public void Enter(StampedMap constants, IInteraction interaction)
        {
            if ((constants, DirectoryToServeConst).IsRereadRequired(out FileInWorkingDirectory? candidate) && candidate is FileInWorkingDirectory workingFile)
            {
                this.DirectoryToServe = workingFile.ToString();
            }
            else if ((constants, DirectoryToServeConst).IsRereadRequired(out string? newDirectory) && !string.IsNullOrWhiteSpace(newDirectory))
            {
                this.DirectoryToServe = newDirectory;
            }
            if ((constants, DefaultIndexFilesConst).IsRereadRequired(out object? indexCandidates) && indexCandidates != null)
            {
                if (indexCandidates is string singleCandidate)
                    this.DefaultIndexFiles = [singleCandidate];
                else if (indexCandidates is IEnumerable multipleCandidates)
                    this.DefaultIndexFiles = multipleCandidates.OfType<string>().ToArray();
                else
                    this.DefaultIndexFiles = [];

            }
            if ((constants, DefaultIndexFilesConst).IsRereadRequired(out object[]? candidateIndexFiles) && candidateIndexFiles != null)
            {
                this.DefaultIndexFiles = candidateIndexFiles.OfType<string>().ToArray();
            }
            if ((constants, RunProgramsConst).IsRereadRequired(out bool? runProgramsCandidate))
            {
                this.RunPrograms = runProgramsCandidate == true;
            }

            if (string.IsNullOrWhiteSpace(this.DirectoryToServe))
            {
                OnException?.Invoke(this, new CommonInteraction(interaction, "No newDirectory specified for fileserver."));
                return;
            }
            if (interaction is not RelativeRouteInteraction routeEval || routeEval.HttpHead is not HttpHeadInteraction head)
            {
                OnException?.Invoke(this, new CommonInteraction(interaction, "Routing information is not valid for file serving."));
                return;
            }

            var components = routeEval.Remaining.ToArray();
            var filePath = Path.Combine(this.DirectoryToServe, string.Join(System.IO.Path.DirectorySeparatorChar, components));
            // var directoryPath = Path.GetDirectoryName(filePath);
            if (File.Exists(filePath))
            {
                HandleFileFound(interaction, new FileInfo(filePath));
            }
            else if (Directory.Exists(filePath) && DefaultIndexFiles.Length > 0)
            {
                var dirInfo = new DirectoryInfo(filePath);
                var matchingIndexFiles = dirInfo.
                    GetFiles().
                    Join(DefaultIndexFiles, x => x.Name.ToLower(), x => x.ToLower(), (fileInfo, indexFile) => (fileInfo, indexFile)).
                    ToArray();
                if (matchingIndexFiles.Length > 0)
                {
                    HandleFileFound(interaction, matchingIndexFiles.First().fileInfo);
                }
            }
        }

        private void HandleFileFound(IInteraction interaction, FileInfo fileInfo)
        {
            if (fileInfo.Extension.ToLower().EndsWith("rkop"))
            {
                if (RunPrograms)
                {
                    var caller = new Call();
                    caller.OnThen += OnThen;
                    caller.OnElse += OnElse;
                    caller.Enter(new StampedMap(fileInfo.FullName), interaction);
                }
                else
                {
                    OnElse?.Invoke(this, interaction);
                }
            }
            else
            {
                var printer = new PrintFile();
                printer.OnException += OnException;
                printer.Enter(new StampedMap(fileInfo.FullName, new SwitchingDictionary(["setlength"], key => key switch
                {
                    "setlength" => true,
                    _ => throw new KeyNotFoundException(),
                })), interaction);
            }
        }

        public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
    }
}
