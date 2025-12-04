namespace Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;

public interface IRelativeRouteInteraction : IInteraction
{
    string CurrentLocation { get; }
    IEnumerable<string> Remaining { get; }
}