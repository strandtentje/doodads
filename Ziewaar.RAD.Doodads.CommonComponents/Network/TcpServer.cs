using System.Net;
using System.Net.Sockets;
using System.Threading;
using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;

namespace Ziewaar.RAD.Doodads.Cryptography;
#nullable enable
public class TcpServer : IService
{
    private readonly UpdatingPrimaryValue RepeatNameConstant = new();
    private string? CurrentRepeatName;
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnIpEndpoint;
    public event CallForInteraction? OnClient;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (!(constants, RepeatNameConstant).IsRereadRequired(out string? repeatNameCandidate))
            this.CurrentRepeatName = repeatNameCandidate;
        if (string.IsNullOrWhiteSpace(this.CurrentRepeatName))
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Repeat name required"));
            return;
        }

        var tsi = new TextSinkingInteraction(interaction);
        OnIpEndpoint?.Invoke(this, tsi);
        string endpointString = tsi.ReadAllText();

        IPEndPoint listenEndpoint;
        var splitEndpoint = endpointString.Split(':');
        var address = splitEndpoint.ElementAtOrDefault(0);
        var port = splitEndpoint.ElementAtOrDefault(1);

        if (address == null || port == null || !IPAddress.TryParse(address, out IPAddress ipAddress) ||
            !ushort.TryParse(port, out ushort portNumber))
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Endpoint string badly formatted"));
            return;
        }
        else
        {
            listenEndpoint = new IPEndPoint(ipAddress, portNumber);
        }

        var repeater = new RepeatInteraction(CurrentRepeatName, interaction);
        repeater.IsRunning = true;
        using var ingestAnother = new Semaphore(0, 1);
        TcpListener listener = new(listenEndpoint);
        listener.Start();
        try
        {
            while (repeater.IsRunning)
            {
                repeater.IsRunning = false;
                listener.BeginAcceptTcpClient(NewTcpClientHandler,
                    new TcpServerClientAcquired(listener, interaction, ingestAnother));
                ingestAnother.WaitOne();
                OnThen?.Invoke(this, repeater);
            }
        }
        finally
        {
            listener.Stop();
        }
    }
    private void NewTcpClientHandler(IAsyncResult ar)
    {
        if (ar.AsyncState is not TcpServerClientAcquired asyncResult)
        {
            OnException?.Invoke(this, new CommonInteraction(StopperInteraction.Instance, "Strange TCP client result"));
            return;
        }
        else if (this.CurrentRepeatName is not string repeatName || string.IsNullOrWhiteSpace(repeatName))
        {
            OnException?.Invoke(this, new CommonInteraction(asyncResult.Origin, "Repeat name missing"));
            return;
        }

        TcpClient freshClient;
        try
        {
            freshClient = asyncResult.Listener.EndAcceptTcpClient(ar);
        }
        catch (Exception ex)
        {
            OnException?.Invoke(this, new CommonInteraction(asyncResult.Origin, ex));
            return;
        }
        finally
        {
            asyncResult.IngestAnother.Release();
        }

        using (freshClient)
        {
            var clientInteraction =
                new TcpClientDuplexInteraction(asyncResult.Origin, freshClient, freshClient.GetStream());
            OnClient?.Invoke(this, clientInteraction);
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}