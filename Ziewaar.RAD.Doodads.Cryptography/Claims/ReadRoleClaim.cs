using System.Security.Claims;

namespace Ziewaar.RAD.Doodads.Cryptography;
public class ReadRoleClaim : ReadClaim
{
    public override void Enter(StampedMap constants, IInteraction interaction) =>
        base.Enter(new StampedMap(ClaimsIdentity.DefaultNameClaimType), interaction);
}
