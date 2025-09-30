#pragma warning disable 67

namespace Ziewaar.RAD.Doodads.Cryptography.Ssh.Sessions;

[Category("Networking & Connections")]
[Title("Authenticate an SSH session")]
[Description("""
             Based on the PublicKey the client has ultimately offered for
             authentication, mark the session as authentic (or not) 
             """)]
public class SshSessionPublicKeyAuthentic : IService
{
    [EventOccasion(
        """
        Based on the provided claims, sink a description of the authentication here, 
        or sink nothing to reject it.
        """)]
    public event CallForInteraction? OnThen;

    [EventOccasion(
        """
        When nothing was sunk and the session was rejected.
        """)]
    public event CallForInteraction? OnElse;

    [EventOccasion(
        """
        Likely when there was no session, or no preceeding publickey query was done
        that matches the current public key on offer.
        """)]
    public event CallForInteraction? OnException;

    private static readonly Pkcs8KeyFormatter Formatter = new();

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (!interaction.TryGetClosest<SshSessionInteraction>(
                out var sessionInteraction) || sessionInteraction == null)
        {
            OnException?.Invoke(this,
                interaction.AppendRegister("SSH session interaction required"));
            return;
        }

        sessionInteraction.Session.Authenticating += OnSessionOnAuthenticating;
        return;

        void OnSessionOnAuthenticating(object? s,
            SshAuthenticatingEventArgs e)
        {
            try
            {
                ActualAuthenticationHappening(interaction, sessionInteraction,
                    e);
            }
            finally
            {
                sessionInteraction.Session.Authenticating -=
                    OnSessionOnAuthenticating;
            }
        }
    }

    public void ActualAuthenticationHappening(
        IInteraction interaction,
        SshSessionInteraction sessionInteraction,
        SshAuthenticatingEventArgs args)
    {
        if (args.AuthenticationType !=
            SshAuthenticationType.ClientPublicKey ||
            args.PublicKey == null) return;

        if (!interaction.TryGetClosest<IClaimsReadingInteraction>(
                out var existingClaimsInteraction)
            || existingClaimsInteraction == null)
        {
            OnException?.Invoke(this,
                new CommonInteraction(interaction,
                    "Claims interaction required"));
            return;
        }

        var pemClaims = existingClaimsInteraction.Claims
            .Where(x => x.Type == "publickeypem")
            .ToArray();

        if (pemClaims.Length != 1)
        {
            OnException?.Invoke(this,
                interaction.AppendRegister("No pem previously queried"));
            return;
        }

        var pem = Formatter.Export(args.PublicKey, includePrivate: false)
            .EncodePem();

        if (pemClaims[0].Value != pem)
        {
            OnException?.Invoke(this,
                interaction.AppendRegister("Attempting to auth with pem that " +
                                           "wasn't previously queried."));
            return;
        }

        var authenticationTypeSink =
            new TextSinkingInteraction(interaction);
        OnThen?.Invoke(this, authenticationTypeSink);
        var determinedAuthenticationType =
            authenticationTypeSink.ReadAllText();

        if (string.IsNullOrWhiteSpace(determinedAuthenticationType))
        {
            OnElse?.Invoke(this, interaction);
            return;
        }

        var finalIdentity =
            new ClaimsIdentity(existingClaimsInteraction.Claims,
                determinedAuthenticationType);
        var finalPrincipal =
            new ClaimsPrincipal(finalIdentity);
        args.AuthenticationTask =
            Task.FromResult<ClaimsPrincipal?>(finalPrincipal);
    }

    public void HandleFatal(IInteraction source, Exception ex) =>
        OnException?.Invoke(this, source);
}