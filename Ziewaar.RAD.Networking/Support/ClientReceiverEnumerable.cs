using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;
using Ziewaar.Network.Protocol;

namespace Ziewaar.RAD.Networking;

public class ClientReceiverEnumerable(CancellationToken cancellationToken)
    : IClientReceiver, IEnumerable<(TcpClient Client, ProtocolOverStream Protocol)>
{
    private readonly BlockingCollection<(TcpClient Client, ProtocolOverStream Protocol)> IncomingClients =
        new(new ConcurrentQueue<(TcpClient Client, ProtocolOverStream Protocol)>());

    public void CommunicateWithClient(TcpClient client, ProtocolOverStream protocol) =>
        IncomingClients.Add((client, protocol));

    private bool TryTakeBlocking([NotNullWhen(true)] out TcpClient? Client,
        [NotNullWhen(true)] out ProtocolOverStream? Protocol)
    {
        try
        {
            (Client, Protocol) = IncomingClients.Take(cancellationToken);
            return true;
        }
        catch (OperationCanceledException)
        {
            (Client, Protocol) = (null, null);
            return false;
        }
    }

    public IEnumerator<(TcpClient Client, ProtocolOverStream Protocol)> GetEnumerator()
    {
        while (TryTakeBlocking(out var client, out var protocol))
            yield return (client, protocol);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}