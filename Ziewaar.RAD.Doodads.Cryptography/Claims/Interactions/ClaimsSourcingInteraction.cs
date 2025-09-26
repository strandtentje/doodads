using System.Security.Claims;

namespace Ziewaar.RAD.Doodads.Cryptography.Claims.Interactions;
public class ClaimsSourcingInteraction(IInteraction interaction, ClaimsPrincipal? argsPrincipal)
    : IClaimsReadingInteraction
{
    public IInteraction Stack => interaction;
    public object Register => Stack.Register;
    public IReadOnlyDictionary<string, object> Memory => interaction.Memory;
    public IEnumerable<Claim> Claims { get; } = argsPrincipal?.Claims ?? [];
}