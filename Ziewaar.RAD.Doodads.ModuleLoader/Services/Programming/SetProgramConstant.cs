
using System;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;
using Ziewaar.RAD.Doodads.ModuleLoader.Interactions;
using Ziewaar.RAD.Doodads.RKOP;

namespace Ziewaar.RAD.Doodads.ModuleLoader.Services.Programming;

public class SetProgramConstant : IService
{
    public event EventHandler<IInteraction> OnError;
    public event EventHandler<IInteraction> ConstantName;
    public event EventHandler<IInteraction> ConstantType;
    public event EventHandler<IInteraction> ConstantValue;
    public void Enter(ServiceConstants serviceConstants, IInteraction interaction)
    {
        if (!interaction.TryGetClosest<ServiceChangeInteraction>(out var sci))
        {
            OnError?.Invoke(this, VariablesInteraction.ForError(interaction, "not currently changing a service"));
            return;
        }

        string SourceSetting(EventHandler<IInteraction> forwardSourcing, string name, string fallback) =>
            (this, serviceConstants, interaction, forwardSourcing).SourceSetting(name, fallback);

        var reqName = SourceSetting(ConstantName, "name", "");
        var reqType = SourceSetting(ConstantType, "type", "String");
        var reqValue = SourceSetting(ConstantValue, "value", "0");

        if (!string.IsNullOrWhiteSpace(reqName) && Enum.TryParse<ConstantType>(reqType, out var type))
        {
            sci.Definition.ConstantsDescription.Set(reqName, type, reqValue, sci.Definition.TextScope.WorkingDirectory.FullName);
        }
        else
        {
            //public enum ConstantType { String, Bool, Number, Reference, Path }
            OnError?.Invoke(this, VariablesInteraction.ForError(
                interaction, 
                "constant name cant be empty and type must be String, Bool, Number or Relative Path"));
        }
    }
}
