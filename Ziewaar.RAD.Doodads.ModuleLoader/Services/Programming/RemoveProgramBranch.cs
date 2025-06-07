
using System;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;
using Ziewaar.RAD.Doodads.ModuleLoader.Interactions;

namespace Ziewaar.RAD.Doodads.ModuleLoader.Services.Programming;

public class RemoveProgramBranch : IService
{
    public event EventHandler<IInteraction> OnError; 
    public event EventHandler<IInteraction> BranchName;

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

        sci.Definition.Children.RemoveAll(x =>
        {
            if (x.ConstantsDescription.BranchKey == reqBranchName)
            {
                x.VoidAll();
                return true;
            } else
            {
                return false;
            }
        });
    }
}
