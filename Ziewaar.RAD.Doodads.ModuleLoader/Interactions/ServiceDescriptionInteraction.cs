using System.Collections.Generic;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.RKOP;

namespace Ziewaar.RAD.Doodads.ModuleLoader.Interactions;

public class ServiceDescriptionInteraction(IInteraction parent, string leadingPath, ServiceDescription<ServiceBuilder> definition) : IInteraction
{
    public ServiceDescription<ServiceBuilder> Definition => definition;
    public IInteraction Parent => parent;
    public IReadOnlyDictionary<string, object> Variables { get; } =
    new SortedList<string, object>(1)
    {
        { "servicepath", $"{leadingPath}/{definition.ConstantsDescription.BranchKey}" },
        { "branchname", definition.ConstantsDescription.BranchKey }
    };
}
