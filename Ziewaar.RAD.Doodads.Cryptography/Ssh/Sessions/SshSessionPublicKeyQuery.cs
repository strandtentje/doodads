#pragma warning disable 67

namespace Ziewaar.RAD.Doodads.Cryptography.Ssh.Sessions;

[Category("Networking & Connections")]
[Title("Accept Public Key Query")]
[Description("""
             SSH client upon connecting will query for public keys they may 
             want to use. Use the ChangeClaim services to configure claims about 
             this PublicKey, and use Continue("Name") to acknowledge this public 
             key is accepted. Alternatively you may deny the connection immediately
             if clients are expected to have one public key that is correct, or do
             nothing to have the services upstream signal to the client that no
             auth could happen. Authentication ultimately happens with 
             SshSessionPublicKeyAuthentic
             """)]
public class SshSessionPublicKeyQuery : IService
{
    [PrimarySetting("Name to use with the Continue service")]
    private readonly UpdatingPrimaryValue RepeatNameConstant = new();

    private string? CurrentRepeatName;

    [EventOccasion("Hook up a service here that figures out if a public key is acceptable.")]
    public event CallForInteraction? OnThen;

    [NeverHappens] public event CallForInteraction? OnElse;

    [EventOccasion("Likely happens when no repeat name was provided, or no SSH session was present.")]
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, RepeatNameConstant).IsRereadRequired(
                out string? repeatNameCandidate))
            this.CurrentRepeatName = repeatNameCandidate;
        if (string.IsNullOrWhiteSpace(this.CurrentRepeatName))
        {
            OnException?.Invoke(this,
                new CommonInteraction(interaction, "Repeat name required"));
            return;
        }

        if (!interaction.TryGetClosest<SshSessionInteraction>(
                out var sessionInteraction) || sessionInteraction == null)
        {
            OnException?.Invoke(this,
                new CommonInteraction(interaction,
                    "SSH session interaction required"));
            return;
        }

        var formatter = new Pkcs8KeyFormatter();

        void CurrentSessionPublicKeyQueryIncoming(object? _,
            SshAuthenticatingEventArgs args)
        {
            if (args.AuthenticationType !=
                SshAuthenticationType.ClientPublicKeyQuery
                || args.PublicKey == null) return;
            var pem = formatter.Export(args.PublicKey, includePrivate: false)
                .EncodePem();
            (interaction, this.CurrentRepeatName).RunCancellable(repeatInteraction =>
            {
                var pemInteraction = new ClaimsSinkingInteraction(repeatInteraction,
                [
                    new Claim("publickeypem", pem),
                new Claim("publickeydigest",
                    Convert.ToHexString(
                        SHA256.HashData(
                            args.PublicKey.GetPublicKeyBytes().Array))),
                new Claim(ClaimTypes.Name, args.Username ?? "")
                ]);
                OnThen?.Invoke(this, pemInteraction);
                if (repeatInteraction.IsRunning)
                {
                    var claimsIdentity = new ClaimsIdentity(pemInteraction.Claims);
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                    args.AuthenticationTask =
                        Task.FromResult<ClaimsPrincipal?>(claimsPrincipal);
                    sessionInteraction.Session.Authenticating -=
                        CurrentSessionPublicKeyQueryIncoming;
                }
            });
        }

        sessionInteraction.Session.Authenticating +=
            CurrentSessionPublicKeyQueryIncoming;
    }

    public void HandleFatal(IInteraction source, Exception ex) =>
        OnException?.Invoke(this, source);
}