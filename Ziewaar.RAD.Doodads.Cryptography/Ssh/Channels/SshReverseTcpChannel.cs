namespace Ziewaar.RAD.Doodads.Cryptography.Ssh.Channels;
[Category("Networking & Connections")]
[Title("Engage TCP host offer")]
[Description("""
             Instruct the SSH client to accept a tcp client on its forwarded server 
             """)]
public class SshReverseTcpChannel : IService
{
    [EventOccasion("Network streams for remote TCP server connection come out here")]
    public event CallForInteraction? OnThen;
    [EventOccasion("When the tcp connection has died or never lived")]
    public event CallForInteraction? OnElse;
    [EventOccasion("The server on the client side we wish to connect to")]
    public event CallForInteraction? OnEndpoint;
    [EventOccasion("The port on the client-side server")]
    public event CallForInteraction? OnDestination;
    [EventOccasion("Likely happens when the endpoint/port were badly formatted or there was no SSH session.")]
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
            new SshChannelReceivingStream(channelToRemoteServer);
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