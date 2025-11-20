#nullable enable
namespace Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;

public interface ICsrfTokenSourceInteraction : IInteraction
{
    public ICsrfFields Fields { get; }
}