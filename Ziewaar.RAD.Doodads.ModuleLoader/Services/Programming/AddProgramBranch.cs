
using System;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;
using Ziewaar.RAD.Doodads.ModuleLoader.Interactions;
using Ziewaar.RAD.Doodads.RKOP;

namespace Ziewaar.RAD.Doodads.ModuleLoader.Services.Programming;

public class AddProgramBranch : IService
{
    public event EventHandler<IInteraction> OnError;
    public event EventHandler<IInteraction> BranchName;
    public event EventHandler<IInteraction> TypeName;
    public void Enter(ServiceConstants serviceConstants, IInteraction interaction)
    {
        if (!interaction.TryGetClosest<ServiceChangeInteraction>(out var sci))
        {
            OnError?.Invoke(this, VariablesInteraction.ForError(interaction, "not currently changing a service"));
            return;
        }

        string SourceSetting(EventHandler<IInteraction> forwardSourcing, string name, string fallback) =>
            (this, serviceConstants, interaction, forwardSourcing).SourceSetting(name, fallback);

        var reqBranchName = SourceSetting(BranchName, "branchname", "newbranch");
        var reqTypeName = SourceSetting(TypeName, "typename", "VoidService");

        var newDesc = new ServiceDescription<ServiceBuilder>()
        {
            ServiceTypeName = reqTypeName,
            TextScope = sci.Definition.TextScope
        };
        newDesc.ConstantsDescription.BranchKey = reqBranchName;
        sci.Definition.Children.Add(newDesc);
    }
}
