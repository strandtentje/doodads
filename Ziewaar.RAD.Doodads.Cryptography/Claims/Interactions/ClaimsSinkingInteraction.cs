using System.Security.Claims;

namespace Ziewaar.RAD.Doodads.Cryptography;
public class ClaimsSinkingInteraction(IInteraction interaction, IList<Claim> claims)
    : IInteraction, IClaimsReadingInteraction
{
    public IInteraction Stack => interaction.Stack;
    public object Register => interaction.Register;
    public IReadOnlyDictionary<string, object> Memory => interaction.Memory;
    public IList<Claim> Claims => claims;
    IEnumerable<Claim> IClaimsReadingInteraction.Claims => claims;
}