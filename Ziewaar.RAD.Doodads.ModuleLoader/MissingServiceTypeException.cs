using Ziewaar.RAD.Doodads.RKOP.Exceptions;
using Ziewaar.RAD.Doodads.RKOP.Text;

namespace Ziewaar.RAD.Doodads.ModuleLoader;

[Serializable]
public class MissingServiceTypeException(
    string name, 
    string suggestion) : 
    Exception(
        $"No service type `{name}` was found. " +
        $"Did you mean {suggestion} " + 
        $"or have some assemblies not been loaded ");
