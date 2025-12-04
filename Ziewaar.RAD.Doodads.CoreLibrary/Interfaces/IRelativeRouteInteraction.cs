namespace Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;

public interface IRelativeRouteInteraction : IInteraction
{
    IEnumerable<string> Remaining { get; }
}