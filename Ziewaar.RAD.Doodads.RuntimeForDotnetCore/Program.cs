using Ejije.Logging;
using Ziewaar.RAD.Doodads.CommonComponents.TextTemplating;
using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.Cryptography.Secrets;
using Ziewaar.RAD.Doodads.Data.Services;
using Ziewaar.RAD.Doodads.FormsValidation.Services.UrlEncodedOnly;
using Ziewaar.RAD.Doodads.ModuleLoader.Services;
using Ziewaar.RAD.Doodads.RuntimeForDotnetCore.Bootstrapper;
using Ziewaar.RAD.Doodads.StandaloneWebserver.Services;
using Ziewaar.TtLog.Utilities;
using DataRow = Ziewaar.RAD.Doodads.Data.Services.DataRow;

namespace Ziewaar.RAD.Doodads.RuntimeForDotnetCore
{
#nullable enable
    public class Program
    {
        private static void Main(string[] args)
        {
            SpecialPaths.AppName = "ziewaar-doodads";
            Log.Start();

            var myDir = Path.GetDirectoryName(typeof(Program).Assembly.Location);
            FrameworkTypeAdaptorRepository.Instance.Register(DateOnlyAdaptor.Instance).Register(TimeOnlyAdaptor.Instance);

            BootstrappedStartBuilder
                .Create(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "doodads"))
                .AddAssemblyBy<IService>().AddAssemblyBy<WebServer>().AddAssemblyBy<Template>()
                .AddAssemblyBy<Definition>().AddAssemblyBy<DataQuery>().AddAssemblyBy<HtmlForm>()
                .AddAssemblyBy<LoadSensitive>().AddAssemblyBy<DataRow>()
                .AddFile("site.rkop", myDir != null ? File.ReadAllText(Path.Combine(myDir, "site.rkop")) : "")
                .AddFile("server.rkop", myDir != null ? File.ReadAllText(Path.Combine(myDir, "server.rkop")) : "")
                .AddFile("boot.rkop", myDir != null ? File.ReadAllText(Path.Combine(myDir, "boot.rkop")) : "")
                .AddFile("programs.rkop", myDir != null ? File.ReadAllText(Path.Combine(myDir, "programs.rkop")) : "")
                .SetStarter("boot.rkop").ReadArgs(args).Build().Run();
        }
    }
}