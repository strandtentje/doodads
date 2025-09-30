#pragma warning disable 67

namespace Ziewaar.RAD.Doodads.Cryptography.Ssh.Sessions;
public class SshSessionPublicKeyQuery : IService
{
    private readonly UpdatingPrimaryValue RepeatNameConstant = new();
    private string? CurrentRepeatName;
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, RepeatNameConstant).IsRereadRequired(out string? repeatNameCandidate))
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

        void CurrentSessionPublicKeyQueryIncoming(object? _, SshAuthenticatingEventArgs args)
        {
            if (args.AuthenticationType != SshAuthenticationType.ClientPublicKeyQuery 
                || args.PublicKey == null) return;
            var pem = formatter.Export(args.PublicKey, includePrivate: false).EncodePem();
            var repeatInteraction = new RepeatInteraction(this.CurrentRepeatName, interaction) { IsRunning = false };
            var pemInteraction = new ClaimsSinkingInteraction(repeatInteraction, [new Claim("publickeypem", pem), new Claim(ClaimTypes.Name, args.Username ?? "")]);
            OnThen?.Invoke(this, pemInteraction);
            if (repeatInteraction.IsRunning)
            {
                var claimsIdentity = new ClaimsIdentity(pemInteraction.Claims);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                args.AuthenticationTask = Task.FromResult<ClaimsPrincipal?>(claimsPrincipal);
                sessionInteraction.Session.Authenticating -= CurrentSessionPublicKeyQueryIncoming;
            }
        }

        sessionInteraction.Session.Authenticating += CurrentSessionPublicKeyQueryIncoming;
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}