namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services;
#pragma warning disable 67
public class ReadUrlEncodedBody : IService
{
    private readonly UpdatingPrimaryValue BodyNamesWhitelistConstant = new();
    private string[] WhitelistedFieldNames = [];
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, BodyNamesWhitelistConstant).IsRereadRequired(() => Enumerable.Empty<string>(),
                out var candidateWhitelist))
        {
            if (candidateWhitelist != null)
            {
                this.WhitelistedFieldNames = candidateWhitelist.ToArray();
            }
            else
            {
                this.WhitelistedFieldNames = [];
            }
        }

        var currentWhiteList = WhitelistedFieldNames;
        if (currentWhiteList.Length == 0)
        {
            if (interaction.TryGetClosest<CsrfTokenSourceInteraction>(out var csrfInteraction) &&
                interaction.TryGetClosest<PreValidationStateInteraction>(out var prevalidation) &&
                csrfInteraction != null && prevalidation != null)
                currentWhiteList = csrfInteraction.Fields.GetWhitelist(prevalidation.FormName);
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
        OnThen?.Invoke(this, new CommonInteraction(interaction, memory: new UrlEncodedQueryDictionary(
            urlEncodedBody, currentWhiteList.Select(HttpUtility.UrlEncode).OfType<string>().ToArray(), new())));
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}