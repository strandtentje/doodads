using System;
using System.Collections.Generic;
using Ziewaar.RAD.Doodads.CoreLibrary.Attributes;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;
using Ziewaar.RAD.Doodads.ModuleLoader.Interactions;

namespace Ziewaar.RAD.Doodads.ModuleLoader.Services;
public class Definition : IService
{    
    public event EventHandler<IInteraction> OnError;
    [DefaultBranch]
    public event EventHandler<IInteraction> Begin;
    public void Enter(ServiceConstants serviceConstants, IInteraction interaction)
    {
        if (interaction.TryGetClosest<CallingInteraction>(out var ci))
        {
            Begin?.Invoke(this, new VariablesInteraction(interaction, serviceConstants));
        } else if (interaction.TryGetClosest<ISelfStartingInteraction>(out var ss))
        {
            Begin?.Invoke(this, new VariablesInteraction(interaction, serviceConstants));
        }
    }
}
