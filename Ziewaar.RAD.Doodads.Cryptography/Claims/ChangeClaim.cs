#pragma warning disable 67

namespace Ziewaar.RAD.Doodads.Cryptography.Claims;

[Category("Authentication & Authorization")]
[Title("Change a claim by type")]
[Description("""
             Provided a claims context via ie. SSH-related services, 
             access a claim value by its type. The type to change, 
             is to be in the Register.
             """)]
public class ChangeClaim : IService
{
    [PrimarySetting("The claim type to change")]
    private readonly UpdatingPrimaryValue ClaimTypeOverrideConst = new();
    private string? CurrentClaimTypeOverride;
    [EventOccasion("Sink the new claim value here.")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [EventOccasion("""
                   Likely happens when there was no claims context, or the claim 
                   type to alter could not be found.
                   """)]
    public event CallForInteraction? OnException;

    public virtual void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, ClaimTypeOverrideConst).IsRereadRequired(
                out string? candidateClaimType))
            this.CurrentClaimTypeOverride = candidateClaimType;

        if (!interaction.TryGetClosest<ClaimsSinkingInteraction>(
                out var claimsInteraction) ||
            claimsInteraction == null)
        {
            OnException?.Invoke(this,
                new CommonInteraction(interaction,
                    "Claims interaction required"));
            return;
        }

        string claimType;
        if (this.CurrentClaimTypeOverride is string overrideType)
            claimType = overrideType;
        else if (interaction.Register.ToString() is string dynamicType)
            claimType = dynamicType;
        else
        {
            OnException?.Invoke(this,
                new CommonInteraction(interaction, "Claim type required"));
            return;
        }

        var tsi = new TextSinkingInteraction(interaction);
        OnThen?.Invoke(this, tsi);
        var newValue = tsi.ReadAllText();

        if (claimsInteraction.Claims.SingleOrDefault(x => x.Type == claimType)
            is
            { } existingClaim)
            claimsInteraction.Claims.Remove(existingClaim);
        claimsInteraction.Claims.Add(new(claimType ?? string.Empty, newValue));
    }

    public void HandleFatal(IInteraction source, Exception ex) =>
        OnException?.Invoke(this, source);
}