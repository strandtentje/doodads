using System.Net.Sockets;
using System.Threading;

namespace Ziewaar.RAD.Doodads.Cryptography;

public class TcpServerClientAcquired(
    TcpListener listener,
    IInteraction origin,
    Semaphore ingestAnother)
{
    public TcpListener Listener => listener;
    public IInteraction Origin => origin;
    public Semaphore IngestAnother => ingestAnother;
}