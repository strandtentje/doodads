using System.Net;
using System.Net.Sockets;
using Ziewaar.Network.Memory;
using Ziewaar.Network.Protocol;
using Ziewaar.RAD.Doodads.CoreLibrary;
using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;

namespace Ziewaar.RAD.Networking;

public class FromTcpListener : IService
{
    private readonly UpdatingKeyValue PortNumberConstant = new UpdatingKeyValue("port");
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (GlobalLog.Instance is not { } logger ||
            !constants.NamedItems.TryGetValue("port", out object? portNumberCandidate) ||
            portNumberCandidate?.ToString() is not { } portNumberString ||
            !ushort.TryParse(portNumberString, out var portNumber))
            throw new Exception("Runtime with logger required.");
        else
        {
            var nullProtocol = new ProtocolOverStreamFactory(GlobalLog.Instance, new StructMemoryPool(),
                new MessageTypeNames(),
                new ProtocolDefinition("NULL", 0));

            var ri = new RepeatInteraction(
                constants.PrimaryConstant.ToString() ?? throw new Exception("Missing repeat name"), interaction,
                new CancellationToken(false));

            EventWaitHandle breakdown = new EventWaitHandle(false, EventResetMode.ManualReset);
            var receiver = new ClientReceiver(ReceiveConnectionCallback);

            using var listener = new TcpListener(IPAddress.Any, portNumber);
            listener.Start();
            using ClientEmitter ce = new(logger, listener, nullProtocol, receiver);
            ce.Start();
            breakdown.WaitOne();

            void ReceiveConnectionCallback(TcpClient client, ProtocolOverStream protocol)
            {
                ri.IsRunning = false;
                OnThen?.Invoke(this, new DuplexInteraction(ri, client.GetStream()));
                if (ri.IsRunning == false)
                {
                    using (breakdown)
                        breakdown.Set();
                }
            }
        }
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}