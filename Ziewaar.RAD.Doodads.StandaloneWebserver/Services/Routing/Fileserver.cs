using Ziewaar.RAD.Doodads.CommonComponents.LiteralSource;
using Ziewaar.RAD.Doodads.ModuleLoader.Services;
using Ziewaar.RAD.Doodads.RKOP.Constructor;

namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services.Routing
{
    [Category("HTTP Routing")]
    [Title("Routing to files based on filesystem")]
    [Description("""
        By default, when combined with Route(), will serve files relative to that url, out of the 
        directory configured in the primary setting. Content Types and Content Lengths will be set.

        To enable dealing with directory urls, indexfiles may be configured with an array of filenames
        that can handle directory roots.

        .rkop files will not be served statically or otherwise by default, but running them 
        may be enabled by setting "run" to true.
        """)]
    public class Fileserver : IService
    {
        [PrimarySetting("Set the directory path to serve here")]
        private readonly UpdatingPrimaryValue DirectoryToServeConst = new UpdatingPrimaryValue();
        [NamedSetting("indexfiles", "array of filenames that may be considered for serving a directory index; the first filename takes highest precedence")]
        private readonly UpdatingKeyValue DefaultIndexFilesConst = new UpdatingKeyValue("indexfiles");
        [NamedSetting("run", "Enable this to execute rkop files, otherwise, rkop files will not be served; statically or otherwise.")]
        private readonly UpdatingKeyValue RunProgramsConst = new UpdatingKeyValue("run");

        private string? DirectoryToServe;
        private string[] DefaultIndexFiles = [];
        private bool RunPrograms;

        [EventOccasion("Only when `run` is enabled; will pass through Returns from rkop files")]
        public event CallForInteraction? OnThen;
        [EventOccasion("Only when `run` is enabled; will pass through Returns from rkop files")]
        public event CallForInteraction? OnElse;
        [EventOccasion("Likely when the directory wasn't configured right, or this wasn't used in conjunction with Route")]
        public event CallForInteraction? OnException;
        [EventOccasion("When the file was not found whatsoever")]
        public event CallForInteraction? NotFound;

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
                return;
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
                    return;
                }
            }
            NotFound?.Invoke(this, interaction);
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
                var printer = new PrintContent();
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
