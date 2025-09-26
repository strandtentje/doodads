using System.Security.Claims;

namespace Ziewaar.RAD.Doodads.Cryptography.Claims.Interactions;
public interface IClaimsReadingInteraction : IInteraction
{
    IEnumerable<Claim> Claims { get; }
}