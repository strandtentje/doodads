using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;
using Ziewaar.Network.Protocol;

namespace Ziewaar.RAD.Networking;

public class ClientReceiver(CancellationToken cancellationToken, Action<TcpClient, ProtocolOverStream> callback)
    : IClientReceiver
{
    public void CommunicateWithClient(TcpClient client, ProtocolOverStream protocol) => callback(client, protocol);
}