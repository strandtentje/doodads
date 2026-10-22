#nullable enable
namespace Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;

public interface ICsrfTokenSourceInteraction : IInteraction
{
    public string RawCsrfCookie { get; }
    public ICsrfFields Fields { get; }
}