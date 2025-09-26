using Microsoft.DevTunnels.Ssh.Events;
using Microsoft.DevTunnels.Ssh.Keys;
using System.Security.Claims;
using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;

namespace Ziewaar.RAD.Doodads.Cryptography;

public class SshSessionPublicKeyAuthentication : IService
{
    private readonly UpdatingPrimaryValue RepeatNameConstant = new();
    private string? CurrentRepeatName;
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
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

        sessionInteraction.Session.Authenticating += (sender, args) =>
        {
            if (args.AuthenticationType != SshAuthenticationType.ClientPublicKey || args.PublicKey == null)
                return;
            var pem = formatter.Export(args.PublicKey, includePrivate: false).EncodePem();
            var repeatInteraction = new RepeatInteraction(this.CurrentRepeatName, interaction) { IsRunning = false };
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
        };
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}