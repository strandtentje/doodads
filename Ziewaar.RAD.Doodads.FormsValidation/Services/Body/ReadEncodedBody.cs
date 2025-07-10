using Ziewaar.RAD.Doodads.FormsValidation.Common;
using Ziewaar.RAD.Doodads.FormsValidation.Interactions;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.Body;
#pragma warning disable 67
public abstract class ReadEncodedBody<TDictionary> : IService where TDictionary : IDecodingDictionary
{
    [PrimarySetting("Array of field names that are accepted. Leave empty to use CSRF obfuscated field names")]
    private readonly UpdatingPrimaryValue BodyNamesWhitelistConstant = new();
    private string[] WhitelistedFieldNames = [];
    [EventOccasion("Puts body values in memory either under whitelist names, or under CSRF obfuscated field names")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [EventOccasion("""
                   Likely happens when no data could be read, or no field names could be determined via 
                   either the primary constant or the CSRF names
                   """)]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, BodyNamesWhitelistConstant).IsRereadRequired(() => Enumerable.Empty<object>(),
                out var candidateWhitelist))
        {
            if (candidateWhitelist != null)
            {
                this.WhitelistedFieldNames = candidateWhitelist.OfType<string>().ToArray();
            }
            else
            {
                this.WhitelistedFieldNames = [];
            }
        }

        var currentWhiteList = WhitelistedFieldNames;
        if (currentWhiteList.Length == 0)
        {
            if (interaction.TryGetClosest<ICsrfTokenSourceInteraction>(out var csrfInteraction) &&
                interaction.TryGetClosest<PreValidationStateInteraction>(out var prevalidation) &&
                csrfInteraction != null && prevalidation != null)
                currentWhiteList = csrfInteraction.Fields.GetObfuscatedWhitelist(prevalidation.FormName);
            else
            {
                OnException?.Invoke(this,
                    new CommonInteraction(interaction,
                        """
                        either a named form and csrf is required, or set a field name whitelist as the primary setting.
                        setting a field name white list will not work in case you're getting this message from within
                        a csrf + form validation block.
                        """));
                return;
            }
        }

        if (!interaction.TryGetClosest<ISourcingInteraction>(out var sourcingInteraction) ||
            sourcingInteraction == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction,
                """
                URL Encoded body reading requires a sourcing interaction from ie. a webserver.
                """));
            return;
        }
        string urlEncodedBody = "";
        using (var reader = new StreamReader(sourcingInteraction.SourceBuffer, sourcingInteraction.TextEncoding))
            urlEncodedBody = reader.ReadToEnd();
        OnThen?.Invoke(this,
            new CommonInteraction(interaction, memory: TDictionary.CreateFor(urlEncodedBody, currentWhiteList)));
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}