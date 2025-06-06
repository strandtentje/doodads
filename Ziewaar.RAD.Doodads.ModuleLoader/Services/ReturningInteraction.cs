
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Ziewaar.RAD.Doodads.CoreLibrary.Attributes;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;
using Ziewaar.RAD.Doodads.RKOP;

namespace Ziewaar.RAD.Doodads.ModuleLoader.Services;
public class ReturningInteraction(IInteraction parent, CallingInteraction cause, SortedList<string, object> variables)
    : VariablesInteraction(parent, variables)
{
    public CallingInteraction Cause => cause;
}

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
                    x => x.ConstantsDescription.Key == moduleRoute.Dequeue());
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
            if (item.ConstantsDescription.Key.StartsWith("_"))
                DefineRedirect?.Invoke(this, new ServiceDescriptionInteraction(interaction, pathSoFar, item));
            else
                Branch?.Invoke(this, new ServiceDescriptionInteraction(interaction, pathSoFar, item));
        }
    }
}

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
            sci.Definition.ConstantsDescription.Set(reqName, type, reqValue, sci.Definition.WorkingDirectory?.FullName ?? "");
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
            WorkingDirectory = sci.Definition.WorkingDirectory,
        };
        newDesc.ConstantsDescription.Key = reqBranchName;
        sci.Definition.Children.Add(newDesc);
    }
}
public static class ServiceDescriptionExtensions
{
    public static void Cleanup(this ServiceDescription<ServiceBuilder> desc)
    {
        foreach (var item in desc.Children)
        {
            item.Cleanup();
        }
        desc.SingleBranch?.Cleanup();
        desc.Concatenation?.Cleanup();
        desc.Wrapper.Cleanup();
    }
}

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
            if (x.ConstantsDescription.Key == reqBranchName)
            {
                x.Cleanup();
                return true;
            } else
            {
                return false;
            }
        });
    }
}

public class ServiceDescriptionInteraction(IInteraction parent, string leadingPath, ServiceDescription<ServiceBuilder> definition) : IInteraction
{
    public ServiceDescription<ServiceBuilder> Definition => definition;
    public IInteraction Parent => parent;
    public IReadOnlyDictionary<string, object> Variables { get; } =
    new SortedList<string, object>(1)
    {
        { "servicepath", $"{leadingPath}/{definition.ConstantsDescription.Key}" },
        { "branchname", definition.ConstantsDescription.Key }
    };
}

public class ServiceChangeInteraction(IInteraction parent, ServiceDescription<ServiceBuilder> definition, KnownProgram program) : IInteraction
{
    public ServiceDescription<ServiceBuilder> Definition => definition;
    public KnownProgram Program => program;
    public IInteraction Parent => parent;
    public IReadOnlyDictionary<string, object> Variables => throw new NotImplementedException();
}
