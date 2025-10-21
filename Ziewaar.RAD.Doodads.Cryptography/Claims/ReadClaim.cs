namespace Ziewaar.RAD.Doodads.Cryptography.Claims;
[Category("Authentication & Authorization")]
[Title("Read claim by type")]
[Description("Works like ChangeClaim except sticks the value in the Register at OnThen")]
public class ReadClaim : IService
{
    [PrimarySetting("Fixed claim type to read from")]
    private readonly UpdatingPrimaryValue ClaimTypeOverrideConst = new();
    private string? CurrentClaimTypeOverride;
    [EventOccasion("When the claim value is available in register")]
    public event CallForInteraction? OnThen;
    [EventOccasion("When no such thing existed as a claim")]
    public event CallForInteraction? OnElse;
    [EventOccasion("""
                   Likely happens when there was no claims context, or the claim type could not be determined
                   """)]
    public event CallForInteraction? OnException;
    public virtual void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, ClaimTypeOverrideConst).IsRereadRequired(out string? candidateClaimType))
            this.CurrentClaimTypeOverride = candidateClaimType;

        if (!interaction.TryGetClosest<IClaimsReadingInteraction>(out var claimsInteraction) ||
            claimsInteraction == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Claims interaction required"));
            return;
        }

        string claimType;
        if (this.CurrentClaimTypeOverride is string overrideType)
            claimType = overrideType;
        else if (interaction.Register.ToString() is string dynamicType)
            claimType = dynamicType;
        else
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Claim type required"));
            return;
        }

        var claim = claimsInteraction.Claims.SingleOrDefault(x => x.Type == claimType);

        if (claim == null) OnElse?.Invoke(this, interaction);
        else OnThen?.Invoke(this, new CommonInteraction(interaction, claim.Value));
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}