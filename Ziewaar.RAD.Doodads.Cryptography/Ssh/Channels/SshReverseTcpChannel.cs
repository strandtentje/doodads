using System.Net;
using Microsoft.DevTunnels.Ssh;
using Ziewaar.RAD.Doodads.Cryptography.Claims.Interactions;
using Ziewaar.RAD.Doodads.Cryptography.Ssh.Channels.Support;
using Ziewaar.RAD.Doodads.Cryptography.Ssh.Sessions.Support;

namespace Ziewaar.RAD.Doodads.Cryptography.Ssh.Channels;
public class SshReverseTcpChannel : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnEndpoint;
    public event CallForInteraction? OnDestination;
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
        string endpointText = endpointSink.ReadAllText();
        if (!IPEndPoint.TryParse(endpointText, out var connectedEndpoint))
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Badly formatted connected endpoint"));
            return;
        }

        var destinationSink = new TextSinkingInteraction(claimsInteraction);
        OnDestination?.Invoke(this, destinationSink);
        string destinationText = destinationSink.ReadAllText();
        if (!IPEndPoint.TryParse(destinationText, out var originEndpoint))
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Badly formatted destination endpoint"));
            return;
        }

        if (sessionInteraction.Session.IsClosed)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "SSH session closed"));
            return;
        }

        if (!sessionInteraction.Session.IsConnected)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "SSH not connected"));
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

        var diagInteraction = new CommonInteraction(interaction,
            memory: new SortedList<string, object>() { ["channelid"] = channelToRemoteServer.ChannelId, });
        var sourcing = new SshReceiveSourcingInteraction(diagInteraction, receivingSshStream);
        var combined = new SshReceiveSinkingInteraction(sourcing, sendingSshStream);

        try
        {
            OnThen?.Invoke(this, combined);
        }
        finally
        {
            OnElse?.Invoke(this, claimsInteraction);
            if (!channelToRemoteServer.IsClosed)
                channelToRemoteServer.CloseAsync().Wait();
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}