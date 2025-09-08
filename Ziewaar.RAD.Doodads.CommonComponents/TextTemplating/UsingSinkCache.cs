#nullable enable
using System.Collections;

namespace Ziewaar.RAD.Doodads.CommonComponents.TextTemplating;

[Category("Printing & Formatting")]
[Title("Cache by keys, invalidate by keys")]
[Description("Use with ReadCache & WriteCache to associate sinking stream data with keys and validation keys.")]
public class UsingSinkCache : IService
{
    private readonly SortedList<CacheKey, CacheValue> Items = new();
    private readonly UpdatingKeyValue CacheIdentityKeysConstant = new("keys");
    private readonly UpdatingKeyValue CacheValidateKeysConstant = new("validatekeys");
    private object[] CurrentIdentityKeys = [];
    private object[] CurrentValidateKeys = [];
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, CacheIdentityKeysConstant).IsRereadRequired(out object? identityCandidate))
            this.CurrentIdentityKeys = identityCandidate switch
            {
                not string and IEnumerable identityCandidateEnumerable => identityCandidateEnumerable.OfType<object>()
                    .Select(FileInWorkingDirectoryUtilities.ToStringOrDatedFile).ToArray(),
                { } identityCandidateMisc => [FileInWorkingDirectoryUtilities.ToStringOrDatedFile(identityCandidateMisc)],
                _ => [],
            };
        if ((constants, CacheValidateKeysConstant).IsRereadRequired(out object? validateCandidate))
            this.CurrentValidateKeys = validateCandidate switch
            {
                not string and IEnumerable validateCandidateEnumerable => validateCandidateEnumerable.OfType<object>()
                    .Select(FileInWorkingDirectoryUtilities.ToStringOrDatedFile).ToArray(),
                { } validateCandidateMisc => [FileInWorkingDirectoryUtilities.ToStringOrDatedFile(validateCandidateMisc)],
                _ => [],
            };

        List<object> identityValues = new();
        foreach (string currentIdentityKey in CurrentIdentityKeys)
            if (interaction.TryFindVariable(currentIdentityKey, out object? identityValue) && identityValue != null)
                identityValues.Add(identityValue);
        List<object> validationValues = new();
        foreach (string currentValidationKey in CurrentValidateKeys)
            if (interaction.TryFindVariable(currentValidationKey, out object? validationValue) &&
                validationValue != null)
                validationValues.Add(validationValue);
        if (identityValues.Count == 0)
        {
            OnException?.Invoke(this,
                new CommonInteraction(interaction,
                    $"no identity values were retrieved for the keys: {string.Join(", ", CurrentIdentityKeys)}"));
            return;
        }

        if (validationValues.Count == 0)
        {
            OnException?.Invoke(this,
                new CommonInteraction(interaction,
                    $"no invalidate values were retrieved for the keys: {string.Join(", ", CurrentValidateKeys)}"));
            return;
        }

        var key = new CacheKey(identityValues);
        bool cacheMiss = false;
        if (!Items.TryGetValue(key, out CacheValue? value) || value is null ||
            !value.TryValidateValues(validationValues))
        {
            Items.Remove(key);
            var cmi = new CacheMissInteraction(interaction, validationValues, key, new CacheValue());
            OnElse?.Invoke(this, cmi);
            if (!cmi.Value.TryValidateValues(validationValues))
            {
                OnException?.Invoke(this,
                    new CommonInteraction(interaction,
                        $"Cache miss & write could not be validated; did you forget CacheWrite?"));
                return;
            }

            Items[key] = value = cmi.Value;
            cacheMiss = true;
        }

        if (interaction.TryGetClosest<ICheckUpdateRequiredInteraction>(
                out ICheckUpdateRequiredInteraction? updateRequiredInteraction) &&
            updateRequiredInteraction != null)
        {
            updateRequiredInteraction.IsRequired = cacheMiss;
        }
        else
        {
            var chi = new CacheHitInteraction(interaction, key, value);
            OnThen?.Invoke(this, chi);
        }
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}