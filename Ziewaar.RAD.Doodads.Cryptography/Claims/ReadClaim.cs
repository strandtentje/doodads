namespace Ziewaar.RAD.Doodads.Cryptography;
public class ReadClaim : IService
{
    private readonly UpdatingPrimaryValue ClaimTypeOverrideConst = new();
    private string? CurrentClaimTypeOverride;
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
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