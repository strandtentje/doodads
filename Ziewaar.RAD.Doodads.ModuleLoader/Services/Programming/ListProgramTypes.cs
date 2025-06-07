
using System;
using Ziewaar.RAD.Doodads.CoreLibrary.Attributes;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;
using Ziewaar.RAD.Doodads.ModuleLoader.Interactions;

namespace Ziewaar.RAD.Doodads.ModuleLoader.Services.Programming;

public class ListProgramTypes : IService
{
    public event EventHandler<IInteraction> OnError;
    [DefaultBranch]
    public event EventHandler<IInteraction> ProgramType;
    public void Enter(ServiceConstants serviceConstants, IInteraction interaction)
    {
        foreach (var item in TypeRepository.Instance.GetAvailableNames())
            ProgramType?.Invoke(this, VariablesInteraction.CreateBuilder(interaction).Add("typename", item).Build());
    }
}

public class ListOpenPrograms : IService
{
    public event EventHandler<IInteraction> OnError;
    [DefaultBranch]
    public event EventHandler<IInteraction> OpenProgram;
    public void Enter(ServiceConstants serviceConstants, IInteraction interaction)
    {
        var kp = ProgramRepository.Instance.GetKnownPrograms();
        foreach (var item in kp)
            OpenProgram?.Invoke(this, new ServiceDescriptionInteraction(
                interaction, "", item, item.DescriptionRoot));
    }
}
