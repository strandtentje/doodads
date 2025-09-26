using System.Security.Claims;

namespace Ziewaar.RAD.Doodads.Cryptography.Claims;
public class ChangeRoleClaim : ChangeClaim
{
    public override void Enter(StampedMap constants, IInteraction interaction) =>
        base.Enter(new StampedMap(ClaimsIdentity.DefaultRoleClaimType), interaction);
}