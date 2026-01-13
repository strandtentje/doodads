using System.Collections;
using System.Collections.Concurrent;
using System.Net.Sockets;
using Ziewaar.Network.Protocol;

namespace Ziewaar.RAD.Networking;

public class NullProtocolClientReceiver(CancellationToken ct) : IClientReceiver, IEnumerable<TcpClient>, IDisposable
{
    private readonly BlockingCollection<TcpClient> PendingClients = new(new ConcurrentQueue<TcpClient>());
    public void CommunicateWithClient(TcpClient client, ProtocolOverStream protocol)
    {
        PendingClients.Add(client);
    }

    public IEnumerator<TcpClient> GetEnumerator()
    {
        while (!ct.IsCancellationRequested)
            yield return PendingClients.Take(ct);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public void Dispose() => PendingClients.Dispose();
}