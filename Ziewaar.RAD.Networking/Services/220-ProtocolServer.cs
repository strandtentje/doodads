using System.Net.Sockets;
using Ziewaar.Network.Protocol;
using Ziewaar.RAD.Doodads.CoreLibrary;
using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;

namespace Ziewaar.RAD.Networking;

public class ProtocolServer : IService, IDisposable
{
    private readonly HashSet<ClientEmitter> OpenEmitters = new();
    private readonly UpdatingPrimaryValue RepeatNameConstant = new UpdatingPrimaryValue();
    private readonly UpdatingKeyValue PortNumberConstant = new("port");
    private readonly UpdatingKeyValue ConnectionLimitConstant = new("connectionlimit");

    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (!interaction.TryGetClosest<CustomInteraction<TcpBasedProtocolFactory>>(out var protocolInteraction) ||
            protocolInteraction == null)
            throw new Exception("This service requires NetworkProtocol service to've run before it.");

        var portNumber = Convert.ToUInt16(constants.NamedItems["port"]);
        var connectionLimit =
            Convert.ToInt32(constants.NamedItems.TryGetValue("connectionlimit", out var connectionLimitCandidate)
                ? connectionLimitCandidate
                : 10000);

        var ri = new RepeatInteraction(constants.PrimaryConstant.ToString() ?? throw new Exception("Missing repeat name"), interaction, new CancellationToken(false));
        
        EventWaitHandle breakdown = new EventWaitHandle(false, EventResetMode.ManualReset);
        var clientReceiver = new ClientReceiver(ReceiveConnectionCallback);
        using var connectionMaker = protocolInteraction.Payload.CreateServer(portNumber, clientReceiver, connectionLimit);
        
        OpenEmitters.Add(connectionMaker);
        
        
        using (connectionMaker)
        {
            try
            {
                connectionMaker.Start();
                breakdown.WaitOne();
                GlobalLog.Instance?.Information("Protocol server broke due to natural reasons");
            }
            catch (Exception ex)
            {
                GlobalLog.Instance?.Warning(ex, "Failed to start enumerable for incoming protocol connections");
            }
            finally
            {
                OpenEmitters.Remove(connectionMaker);
            }
        }

        return;

        void ReceiveConnectionCallback(TcpClient client, ProtocolOverStream protocol)
        {
            ri.IsRunning = false;
            OnThen?.Invoke(this, new CustomInteraction<(TcpClient TcpClient, ProtocolOverStream Protocol)>(ri,
                (client, protocol)));
            if (ri.IsRunning == false)
            {
                using (breakdown) breakdown.Set();
            }
        }
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);

    public void Dispose()
    {
        foreach (var pair in OpenEmitters)
        {
            try
            {
                pair.Dispose();
            }
            catch (Exception ex)
            {
                GlobalLog.Instance?.Warning(ex, "While disposing {type} of {service}", nameof(ClientEmitter),
                    nameof(ProtocolServer));
            }
        }
    }
}