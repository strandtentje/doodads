using System.Collections.Generic;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;

namespace Ziewaar.RAD.Doodads.ModuleLoader;

public class ProgramStarter : ISelfStartingInteraction
{
    public IInteraction Parent => null;
    public IReadOnlyDictionary<string, object> Variables => new SortedList<string, object>();
}
