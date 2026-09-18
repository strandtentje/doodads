using Ejije.Logging;
using Ziewaar.RAD.Doodads.AdvancedFilesystem.DirDsl;
using Ziewaar.RAD.Doodads.CommonComponents.TextTemplating;
using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.Cryptography.Secrets;
using Ziewaar.RAD.Doodads.Data.Services;
using Ziewaar.RAD.Doodads.FormsValidation.Services.UrlEncodedOnly;
using Ziewaar.RAD.Doodads.ModuleLoader.Services;
using Ziewaar.RAD.Doodads.Multimedia;
using Ziewaar.RAD.Doodads.RuntimeForDotnetCore.Bootstrapper;
using Ziewaar.RAD.Doodads.StandaloneWebserver.Services;
using DataRow = Ziewaar.RAD.Doodads.Data.Services.DataRow;
using static Ziewaar.TtLog.Utilities.SpecialPaths;

namespace Ziewaar.RAD.Doodads.RuntimeForDotnetCore
{
    public class Program
    {
        private static readonly Log NoProjectDirectories =
                Log.Oops("File `projects.directory` found in config dir, but no existing path in there existed."),
            NoProjectBootables = Log.Oops("No bootable projects in projects.directory");

        private static void Main(string[] args)
        {
            AppName = "ziewaar-doodads";
            Log.Start();

            FrameworkTypeAdaptorRepository.Instance.Register(DateOnlyAdaptor.Instance)
                .Register(TimeOnlyAdaptor.Instance);

            var projectsFile = Path.Combine(AppDataForAssy, "projects.directory");

            if (File.Exists(projectsFile))
            {
                var projectsDirectories = File.ReadAllLines(projectsFile)
                    .Where(x => !string.IsNullOrWhiteSpace(x) && Directory.Exists(x)).ToArray();
                if (projectsDirectories.Length == 0)
                    Log.Post(NoProjectDirectories);
                var bootables = projectsDirectories.SelectMany(x => RecurseDirectories(x, 4)).ToArray();
                if (bootables.Length == 0)
                    Log.Post(NoProjectBootables);
                List<Thread> threads = new();
                foreach (var bootable in bootables)
                {
                    var thr = new Thread(path =>
                    {
                        using var _ = StartFor((string)path!);
                    });
                    threads.Add(thr);
                    thr.Start(bootable);
                }

                foreach (var thread in threads)
                    thread.Join();
            }
            else
            {
                StartFor(AppDataForAssy);
            }
        }

        private static IDisposable StartFor(string workingDirectory) => BootstrappedStartBuilder
            .Create(workingDirectory)
            .AddAssemblyBy<IService>().AddAssemblyBy<WebServer>().AddAssemblyBy<Template>()
            .AddAssemblyBy<Definition>().AddAssemblyBy<DataQuery>().AddAssemblyBy<HtmlForm>()
            .AddAssemblyBy<LoadSensitive>().AddAssemblyBy<DataRow>().AddAssemblyBy<MatchDir>()
            .AddAssemblyBy<MpvService>()
            .AddFile("site.rkop",
                AssemblyDirectory != null ? File.ReadAllText(Path.Combine(AssemblyDirectory, "site.rkop")) : "")
            .AddFile("server.rkop",
                AssemblyDirectory != null ? File.ReadAllText(Path.Combine(AssemblyDirectory, "server.rkop")) : "")
            .AddFile("boot.rkop",
                AssemblyDirectory != null ? File.ReadAllText(Path.Combine(AssemblyDirectory, "boot.rkop")) : "")
            .AddFile("programs.rkop",
                AssemblyDirectory != null ? File.ReadAllText(Path.Combine(AssemblyDirectory, "programs.rkop")) : "")
            .SetStarter("boot.rkop").ReadArgs([]).Build().Run();

        private static IEnumerable<string> RecurseDirectories(string knownDirectory, int depth)
        {
            if (depth == 0) return [];
            return File.Exists(Path.Combine(knownDirectory, "project.rkop"))
                ? [knownDirectory]
                : Directory.GetDirectories(knownDirectory).SelectMany(x => RecurseDirectories(x, depth - 1));
        }
    }
}