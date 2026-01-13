using System.Net.Sockets;
using Ziewaar.Network.Protocol;
using Ziewaar.RAD.Doodads.CoreLibrary;

namespace Ziewaar.RAD.Networking;

public class ProtocolServer : IteratingService, IDisposable
{
    private readonly HashSet<ClientEmitter> OpenEmitters = new();
    private readonly UpdatingKeyValue PortNumberConstant = new("port");
    private readonly UpdatingKeyValue ConnectionLimitConstant = new("connectionlimit");

    protected override bool RunElse => false;

    protected override IEnumerable<IInteraction> GetItems(StampedMap constants, IInteraction repeater)
    {
        if (!repeater.TryGetClosest<CustomInteraction<TcpBasedProtocolFactory>>(out var protocolInteraction) ||
            protocolInteraction == null)
            throw new Exception("This service requires NetworkProtocol service to've run before it.");
        if (!repeater.TryGetClosest<CancellationInteraction>(out CancellationInteraction? cancellationInteraction))
            cancellationInteraction = null;

        var portNumber = Convert.ToUInt16(constants.NamedItems["port"]);
        var connectionLimit =
            Convert.ToInt32(constants.NamedItems.TryGetValue("connectionlimit", out var connectionLimitCandidate)
                ? connectionLimitCandidate
                : 10000);

        var clientReceiver =
            new ClientReceiverEnumerable(
                cancellationInteraction?.GetCancellationToken() ?? new CancellationToken(false));
        var connectionMaker = protocolInteraction.Payload.CreateServer(portNumber, clientReceiver, connectionLimit);
        OpenEmitters.Add(connectionMaker);
        using (connectionMaker)
        {
            try
            {
                return clientReceiver.Select(x =>
                    new CustomInteraction<(TcpClient TcpClient, ProtocolOverStream Protocol)>(repeater,
                        (x.Client, x.Protocol)));
            }
            finally
            {
                OpenEmitters.Remove(connectionMaker);
            }
        }
    }

    public void Dispose()
    {
        foreach (ClientEmitter clientEmitter in OpenEmitters)
        {
            try
            {
                clientEmitter.Dispose();
            }
            catch (Exception ex)
            {
                GlobalLog.Instance?.Warning(ex, "While disposing {type} of {service}", nameof(ClientEmitter), nameof(ProtocolServer));
            }
        }
    }
}