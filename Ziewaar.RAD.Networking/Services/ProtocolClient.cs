using System.Net.Sockets;
using Ziewaar.Network.Protocol;

namespace Ziewaar.RAD.Networking;

public class ProtocolClient : IService
{
    private readonly UpdatingKeyValue HostnameConstant = new("host");
    private readonly UpdatingKeyValue PortConstant = new("port");

    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (!interaction.TryGetClosest<CustomInteraction<TcpBasedProtocolFactory>>(out var protocolInteraction) ||
            protocolInteraction == null)
            OnException?.Invoke(this,
                interaction.AppendRegister("This service requires NetworkProtocol service to've run before it."));
        else if (!constants.NamedItems.TryGetValue("port", out var portCandidate) ||
                 !constants.NamedItems.TryGetValue("host", out var hostCandidate) ||
                 !ushort.TryParse(portCandidate.ToString(), out var portNumber) ||
                 hostCandidate is not string hostStringCandidate ||
                 string.IsNullOrWhiteSpace(hostStringCandidate))
            OnException?.Invoke(this,
                interaction.AppendRegister("This service requires host=hostname and port=0-65535"));
        else
        {
            var connection = protocolInteraction.Payload.CreateClient(hostStringCandidate, portNumber);
            OnThen?.Invoke(this,
                new CustomInteraction<(TcpClient TcpClient, ProtocolOverStream Protocol)>(interaction,
                    (connection.client, connection.protocol)));
        }
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}