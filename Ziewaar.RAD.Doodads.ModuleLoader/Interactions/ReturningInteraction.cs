using System.Collections.Generic;
using System.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

namespace Ziewaar.RAD.Doodads.ModuleLoader.Interactions;
public class ReturningInteraction(IInteraction parent, CallingInteraction cause, SortedList<string, object> variables)
    : VariablesInteraction(parent, variables)
{
    public CallingInteraction Cause => cause;
}
