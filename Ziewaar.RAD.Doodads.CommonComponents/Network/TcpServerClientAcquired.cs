using System.Net.Sockets;
using System.Threading;
using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;

namespace Ziewaar.RAD.Doodads.CommonComponents.Network;

public class TcpServerClientAcquired(
    TcpListener listener,
    IInteraction origin,
    Semaphore ingestAnother,
    RepeatInteraction repeater)
{
    public TcpListener Listener => listener;
    public IInteraction Origin => origin;
    public Semaphore IngestAnother => ingestAnother;
    public RepeatInteraction Repeater => repeater;
}