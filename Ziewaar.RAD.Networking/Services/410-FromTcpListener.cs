using System.Net;
using System.Net.Sockets;
using Ziewaar.Network.Memory;
using Ziewaar.Network.Protocol;
using Ziewaar.RAD.Doodads.CoreLibrary;

namespace Ziewaar.RAD.Networking;

public class FromTcpListener : IteratingService
{
    private readonly UpdatingKeyValue PortNumberConstant = new UpdatingKeyValue("port");
    protected override bool RunElse => false;
    protected override IEnumerable<IInteraction> GetItems(StampedMap constants, IInteraction repeater)
    {
        if (GlobalLog.Instance is not { } logger || !constants.NamedItems.TryGetValue("port", out object? portNumberCandidate) ||
            portNumberCandidate?.ToString() is not {} portNumberString || !ushort.TryParse(portNumberString, out var portNumber))
            throw new Exception("Runtime with logger required.");
        else
        {
            var ct =
                repeater.TryGetClosest<CancellationInteraction>(out CancellationInteraction? cancellation) &&
                cancellation != null
                    ? cancellation.GetCancellationToken()
                    : new CancellationToken(false);
            
            var nullProtocol = new ProtocolOverStreamFactory(GlobalLog.Instance, new StructMemoryPool(),
                new MessageTypeNames(),
                new ProtocolDefinition("NULL", 0));
            
            using NullProtocolClientReceiver receiver = new NullProtocolClientReceiver(ct);
            using var listener = new TcpListener(IPAddress.Any, portNumber);
            using ClientEmitter ce = new(logger, listener, nullProtocol, receiver);
            ce.Start();

            return receiver.Select(x => new DuplexInteraction(repeater, x.GetStream()));
        }
    }
}