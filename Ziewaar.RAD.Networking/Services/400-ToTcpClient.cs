using System.Net.Sockets;

namespace Ziewaar.RAD.Networking;

public class ToTcpClient : IService, IDisposable
{
    private readonly HashSet<CancellationTokenSource> RunningJobs = new();
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
            using var cts = new CancellationTokenSource();
            var ct = cts.Token;
            using TcpClient client = new TcpClient();
            
            client.Connect(hostName, portNumber);
            var duplex = client.GetStream();
            var outgoingTask = sourcingInteraction.SourceBuffer.CopyToAsync(duplex, ct);
            var incomingTask = duplex.CopyToAsync(sinkingInteraction.SinkBuffer, ct);
            outgoingTask.Wait(ct);
            incomingTask.Wait(ct);
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
    public void Dispose()
    {
        foreach (CancellationTokenSource cancellationTokenSource in RunningJobs)
        {
            using(cancellationTokenSource)
                cancellationTokenSource.Cancel();
        }
    }
}