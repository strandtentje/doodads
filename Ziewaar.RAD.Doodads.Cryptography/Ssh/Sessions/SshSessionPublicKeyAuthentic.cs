using Microsoft.DevTunnels.Ssh.Events;
using Microsoft.DevTunnels.Ssh.Keys;
using System.Security.Claims;
using Ziewaar.RAD.Doodads.Cryptography.Claims.Interactions;
using Ziewaar.RAD.Doodads.Cryptography.Ssh.Sessions.Support;

#pragma warning disable 67

namespace Ziewaar.RAD.Doodads.Cryptography.Ssh.Sessions;

public class SshSessionPublicKeyAuthentic : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (!interaction.TryGetClosest<SshSessionInteraction>(out var sessionInteraction) || sessionInteraction == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "SSH session interaction required"));
            return;
        }

        sessionInteraction.Session.Authenticating += ActualAuthenticationHappening;
        return;

        void ActualAuthenticationHappening(object? _, SshAuthenticatingEventArgs args)
        {
            try
            {
                if (args.AuthenticationType != SshAuthenticationType.ClientPublicKey || args.PublicKey == null) return;

                if (!interaction.TryGetClosest<IClaimsReadingInteraction>(out var existingClaimsInteraction)
                    || existingClaimsInteraction == null)
                {
                    OnException?.Invoke(this, new CommonInteraction(interaction, "Claims interaction required"));
                    return;
                }

                var authenticationTypeSink = new TextSinkingInteraction(interaction);
                OnThen?.Invoke(this, authenticationTypeSink);
                var determinedAuthenticationType = authenticationTypeSink.ReadAllText();

                if (!string.IsNullOrWhiteSpace(determinedAuthenticationType))
                {
                    var finalIdentity =
                        new ClaimsIdentity(existingClaimsInteraction.Claims, determinedAuthenticationType);
                    var finalPrincipal =
                        new ClaimsPrincipal(finalIdentity);
                    args.AuthenticationTask = Task.FromResult<ClaimsPrincipal?>(finalPrincipal);
                }
                else
                {
                    OnElse?.Invoke(this, interaction);
                }
            }
            finally
            {
                sessionInteraction.Session.Authenticating -= ActualAuthenticationHappening;
            }
        }
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}