using System.Net;
using Microsoft.DevTunnels.Ssh;

namespace Ziewaar.RAD.Doodads.Cryptography;
public class SshReverseTcpChannel : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnEndpoint;
    public event CallForInteraction? OnOrigin;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (!interaction.TryGetClosest<SshSessionInteraction>(out var sessionInteraction) || sessionInteraction == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "SSH session interaction required"));
            return;
        }

        var claimsInteraction = new ClaimsSourcingInteraction(interaction, sessionInteraction.Session.Principal);
        var endpointSink = new TextSinkingInteraction(claimsInteraction);
        OnEndpoint?.Invoke(this, endpointSink);
        if (!IPEndPoint.TryParse(endpointSink.ReadAllText(), out var connectedEndpoint))
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Badly formatted connected endpoint"));
            return;
        }

        var originSink = new TextSinkingInteraction(claimsInteraction);
        OnOrigin?.Invoke(this, originSink);
        if (!IPEndPoint.TryParse(connectedEndpoint.Address.ToString(), out var originEndpoint))
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Badly formatted origin endpoint"));
            return;
        }

        using var channelToRemoteServer = sessionInteraction.Session.OpenChannelAsync(
            new SshPfwReqMessage(
                connectedEndpoint.Address.ToString(),
                (uint)connectedEndpoint.Port,
                originEndpoint.Address.ToString(),
                (uint)originEndpoint.Port), initialRequest: null).Result;

        using Stream receivingSshStream =
            new SshChannelReceivingStream(channelToRemoteServer, SshChannel.DefaultMaxWindowSize);
        using Stream sendingSshStream = new SshChannelSendingStream(channelToRemoteServer);

        var sourcing = new SshReceiveSourcingInteraction(interaction, receivingSshStream);
        var combined = new SshReceiveSinkingInteraction(sourcing, sendingSshStream);

        try
        {
            OnThen?.Invoke(this, combined);
        }
        finally
        {
            OnElse?.Invoke(this, claimsInteraction);
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}