using Serilog;
using System.Net.Sockets;
using Ziewaar.RAD.Doodads.CoreLibrary;

namespace Ziewaar.RAD.Networking;

public class ToTcpClient : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (!interaction.TryGetClosest<ISinkingInteraction>(out var sinkingInteraction) ||
            !interaction.TryGetClosest<ISourcingInteraction>(out var sourcingInteraction) ||
            !interaction.TryFindVariable("port", out object? portNumberCandidate) ||
            !interaction.TryFindVariable("host", out string? hostName) ||
            string.IsNullOrWhiteSpace(hostName) ||
            sinkingInteraction == null || sourcingInteraction == null ||
            portNumberCandidate?.ToString() is not { } portNumberString ||
            !ushort.TryParse(portNumberString, out var portNumber))
            OnException?.Invoke(this,
                interaction.AppendRegister(
                    "Expected source and sink to work with, as well as portnumber and hostname variable"));
        else
        {
            using TcpClient client = new TcpClient();

            try
            {
                client.Connect(hostName, portNumber);
                var duplex = client.GetStream();
                
                using (sourcingInteraction.SourceBuffer)
                using (sinkingInteraction.SinkBuffer)
                {
                    var pipeToClient =
                        (sourcingInteraction.SourceBuffer, duplex).CopyGently(() => client.Connected, "pipe to client");
                    var clientToPipe =
                        (duplex, sinkingInteraction.SinkBuffer).CopyGently(() => client.Connected, "client to pipe");
                    
                    pipeToClient.Join();
                    clientToPipe.Join();
                }
            }
            catch (Exception ex)
            {
                GlobalLog.Instance?.Warning(ex, "Streamcopying ceased");
            }
        }
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}