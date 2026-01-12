using System.Net.Sockets;
using Ziewaar.Network.Protocol;

namespace Ziewaar.RAD.Networking;

public class ProtocolServer : IteratingService
{
    private readonly UpdatingPrimaryValue PortNumberConstant = new();
    private readonly UpdatingKeyValue ConnectionLimitConstant = new("connectionlimit");

    protected override bool RunElse => false;

    protected override IEnumerable<IInteraction> GetItems(StampedMap constants, IInteraction repeater)
    {
        if (!repeater.TryGetClosest<CustomInteraction<TcpBasedProtocolFactory>>(out var protocolInteraction) ||
            protocolInteraction == null)
            throw new Exception("This service requires NetworkProtocol service to've run before it.");
        if (!repeater.TryGetClosest<CancellationInteraction>(out CancellationInteraction? cancellationInteraction))
            cancellationInteraction = null;

        var portNumber = Convert.ToUInt16(constants.PrimaryConstant ?? 39933);
        var connectionLimit =
            Convert.ToInt32(constants.NamedItems.TryGetValue("connectionlimit", out var connectionLimitCandidate)
                ? connectionLimitCandidate
                : 10000);

        var clientReceiver =
            new ClientReceiverEnumerable(
                cancellationInteraction?.GetCancellationToken() ?? new CancellationToken(false));
        protocolInteraction.Payload.CreateServer(portNumber, clientReceiver, connectionLimit);

        return clientReceiver.Select(x =>
            new CustomInteraction<(TcpClient TcpClient, ProtocolOverStream Protocol)>(repeater,
                (x.Client, x.Protocol)));
    }
}