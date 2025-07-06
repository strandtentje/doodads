namespace Ziewaar.RAD.Doodads.ModuleLoader.Exceptions;

[Serializable]
public class MissingServiceTypeException(
    string name, 
    string suggestion) : 
    Exception(
        $"No service type `{name}` was found. " +
        $"Did you mean {suggestion} " + 
        $"or have some assemblies not been loaded ");
