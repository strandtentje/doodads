using System.Security.Claims;

namespace Ziewaar.RAD.Doodads.Cryptography;
public interface IClaimsReadingInteraction : IInteraction
{
    IEnumerable<Claim> Claims { get; }
}