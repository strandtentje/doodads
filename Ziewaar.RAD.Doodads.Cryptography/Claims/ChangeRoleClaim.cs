namespace Ziewaar.RAD.Doodads.Cryptography.Claims;
[Category("Authentication & Authorization")]
[Title("Change the name claim")]
[Description("Works like ChangeClaim, except can and will only touch the role claim")]
public class ChangeRoleClaim : ChangeClaim
{
    public override void Enter(StampedMap constants, IInteraction interaction) =>
        base.Enter(new StampedMap(ClaimsIdentity.DefaultRoleClaimType), interaction);
}