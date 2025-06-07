
using System;
using Ziewaar.RAD.Doodads.CoreLibrary.Attributes;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;
using Ziewaar.RAD.Doodads.ModuleLoader.Interactions;

namespace Ziewaar.RAD.Doodads.ModuleLoader.Services.Programming;

public class ChangeProgramType : IService
{
    public event EventHandler<IInteraction> OnError;
    [DefaultBranch]
    public event EventHandler<IInteraction> ProgramType;
    public void Enter(ServiceConstants serviceConstants, IInteraction interaction)
    {
        string SourceSetting(EventHandler<IInteraction> forwardSourcing, string name, string fallback) =>
            (this, serviceConstants, interaction, forwardSourcing).SourceSetting(name, fallback);
        var newTypeName = SourceSetting(ProgramType, "typename", "");
        if (TypeRepository.Instance.HasName(newTypeName) &&
            interaction.TryGetClosest<ServiceChangeInteraction>(out var change))
        {
            change.Definition.ServiceTypeName = newTypeName;
        }
        else
        {
            OnError?.Invoke(this, VariablesInteraction.ForError(interaction, "type doesnt exist, or not changing"));
        }
    }
}
