using Ziewaar.RAD.Doodads.Cryptography.Claims.Interactions;
#pragma warning disable 67

namespace Ziewaar.RAD.Doodads.Cryptography.Claims;
public class ChangeClaim : IService
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

        if (!interaction.TryGetClosest<ClaimsSinkingInteraction>(out var claimsInteraction) ||
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

        var tsi = new TextSinkingInteraction(interaction);
        OnThen?.Invoke(this, tsi);
        var newValue = tsi.ReadAllText();

        if (claimsInteraction.Claims.SingleOrDefault(x => x.Type == claimType) is
            { } existingClaim)
            claimsInteraction.Claims.Remove(existingClaim);
        claimsInteraction.Claims.Add(new(claimType ?? string.Empty, newValue));
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}