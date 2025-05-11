namespace Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;

public interface IInteraction
{
    IInteraction Parent { get; }
    IReadOnlyDictionary<string, object> Variables { get; }
}
