
using System;
using System.Collections.Generic;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;

namespace Ziewaar.RAD.Doodads.ModuleLoader.Services;
public class CallingInteraction(IInteraction offset, Action<IInteraction> continued) : IInteraction
{
    public IInteraction Parent => offset;
    public IReadOnlyDictionary<string, object> Variables => offset.Variables;
    public void Continue(IInteraction interaction) => continued(interaction);
}
