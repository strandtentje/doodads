using System;
using Ziewaar.RAD.Doodads.CoreLibrary.Attributes;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;

namespace Ziewaar.RAD.Doodads.ModuleLoader.Services
{
    public class Call : IService
    {
        [NamedBranch]
        public event EventHandler<IInteraction> OnError;
        [NamedBranch]
        public event EventHandler<IInteraction> Continue;
        [NamedBranch]
        public event EventHandler<IInteraction> ModuleName;
        public void Enter(ServiceConstants serviceConstants, IInteraction interaction)
        {
            string SourceSetting(EventHandler<IInteraction> forwardSourcing, string name, string fallback) =>
                (this, serviceConstants, interaction, forwardSourcing).SourceSetting(name, fallback);

            var moduleName = SourceSetting(ModuleName, "modulefile", "no_module");

            ProgramRepository.Instance.GetEntryPointForFile(moduleName).Run(interaction);
        }
    }
}
