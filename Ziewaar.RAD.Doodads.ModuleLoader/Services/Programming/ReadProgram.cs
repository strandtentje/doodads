
using System;
using System.Collections.Generic;
using System.Linq;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;
using Ziewaar.RAD.Doodads.ModuleLoader.Interactions;
using Ziewaar.RAD.Doodads.RKOP;

namespace Ziewaar.RAD.Doodads.ModuleLoader.Services.Programming;

public class ReadProgram : IService
{
    public event EventHandler<IInteraction> OnError;
    public event EventHandler<IInteraction> ModuleName;
    public event EventHandler<IInteraction> ModuleRoute;

    public event EventHandler<IInteraction> Edit;//
    public event EventHandler<IInteraction> TypeName;//
    public event EventHandler<IInteraction> Concatenation;//
    public event EventHandler<IInteraction> SingleBranch;//
    public event EventHandler<IInteraction> CallRedirect;//
    public event EventHandler<IInteraction> Constant;//
    public event EventHandler<IInteraction> Branch;//
    public event EventHandler<IInteraction> DefineRedirect;//
    public void Enter(ServiceConstants serviceConstants, IInteraction interaction)
    {
        string SourceSetting(EventHandler<IInteraction> forwardSourcing, string name, string fallback) =>
            (this, serviceConstants, interaction, forwardSourcing).SourceSetting(name, fallback);

        var moduleName = SourceSetting(ModuleName, "modulefile", "no_module");

        var program = ProgramRepository.Instance.GetForFile(moduleName);

        ServiceDescription<ServiceBuilder> selectedDefinition;
        string pathSoFar;

        if (interaction.TryGetClosest<ServiceDescriptionInteraction>(out var precedent))
        {
            selectedDefinition = precedent.Definition;
            pathSoFar = (string)precedent.Variables["servicepath"];
        }
        else
        {
            string[] splitPath = SourceSetting(ModuleRoute, "moduleroute", "").Split("/", StringSplitOptions.RemoveEmptyEntries);
            var moduleRoute = new Queue<string>(splitPath);
            selectedDefinition = program.DescriptionRoot;
            while (moduleRoute.Any())
                selectedDefinition = program.DescriptionRoot.Children.SingleOrDefault(
                    x => x.ConstantsDescription.BranchKey == moduleRoute.Dequeue());
            pathSoFar = string.Join("/", splitPath);
        }


        if (selectedDefinition == null)
        {
            OnError?.Invoke(this, VariablesInteraction.ForError(interaction, "branch doesnt exist"));
            return;
        }

        Edit?.Invoke(this, new ServiceChangeInteraction(interaction, selectedDefinition, program));

        var typeVarList = new SortedList<string, object>(1)
        {
            { "typename", selectedDefinition.ServiceTypeName }
        };

        TypeName?.Invoke(this, new VariablesInteraction(interaction, typeVarList));

        foreach (var item in selectedDefinition.ConstantsDescription.Members)
        {
            var settingList = new SortedList<string, object>(1)
            {
                { item.Key, item.Value }
            };
            Constant?.Invoke(this, new VariablesInteraction(interaction, settingList));
        }

        if (selectedDefinition.Concatenation != null)
            Concatenation?.Invoke(this, new ServiceDescriptionInteraction(interaction, pathSoFar, selectedDefinition.Concatenation));

        if (selectedDefinition.SingleBranch != null)
            SingleBranch?.Invoke(this, new ServiceDescriptionInteraction(interaction, pathSoFar, selectedDefinition.SingleBranch));

        if (selectedDefinition.RedirectsTo != null)
            CallRedirect?.Invoke(this, new ServiceDescriptionInteraction(interaction, pathSoFar, selectedDefinition.RedirectsTo));

        foreach (var item in selectedDefinition.Concatenation.Children)
        {
            if (item.ConstantsDescription.BranchKey.StartsWith("_"))
                DefineRedirect?.Invoke(this, new ServiceDescriptionInteraction(interaction, pathSoFar, item));
            else
                Branch?.Invoke(this, new ServiceDescriptionInteraction(interaction, pathSoFar, item));
        }
    }
}
