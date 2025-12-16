#pragma warning disable 67

namespace Ziewaar.RAD.Doodads.Cryptography.Ssh.Sessions.Support;

[Category("Networking & Connections")]
[Title("Authenticate an SSH session by Continue")]
[Description("""
             Seems to do the same as SshSessionPublicKeyAuthentic,
             Except uses Continue to mark one of multiple keys as good,
             and sets the Continue name as the Authentication Type 
             """)]
public class SshSessionPublicKeyAuthentication : IService
{
    [PrimarySetting("Name to Continue with and set as Authentication Type")]
    private readonly UpdatingPrimaryValue RepeatNameConstant = new();
    private string? CurrentRepeatName;
    [EventOccasion("Potential pubkeys come out here and must be accepted with Continue")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [EventOccasion("Likely happens when there was no repat name or no SSH session")]
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (!(constants, RepeatNameConstant).IsRereadRequired(out string? repeatNameCandidate))
            this.CurrentRepeatName = repeatNameCandidate;
        if (string.IsNullOrWhiteSpace(this.CurrentRepeatName))
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Repeat name required"));
            return;
        }

        if (!interaction.TryGetClosest<SshSessionInteraction>(out var sessionInteraction) || sessionInteraction == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "SSH session interaction required"));
            return;
        }

        var formatter = new Pkcs8KeyFormatter();

        sessionInteraction.Session.Authenticating += (_, args) =>
        {
            if (args.AuthenticationType != SshAuthenticationType.ClientPublicKey || args.PublicKey == null)
                return;
            var pem = formatter.Export(args.PublicKey, includePrivate: false).EncodePem();
            (interaction, CurrentRepeatName).RunCancellable(repeatInteraction =>
            {
                var pemInteraction = new ClaimsSinkingInteraction(repeatInteraction, [new Claim("publickeypem", pem)]);
                OnThen?.Invoke(this, pemInteraction);
                if (repeatInteraction.IsRunning)
                {
                    var claimsIdentity = new ClaimsIdentity(pemInteraction.Claims, CurrentRepeatName);
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                    args.AuthenticationTask = Task.FromResult<ClaimsPrincipal?>(claimsPrincipal);
                }
                else
                {
                    args.AuthenticationTask =
                        Task.FromException<ClaimsPrincipal?>(new UnauthorizedAccessException("Public key not recognized."));
                }
            });
        };
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}