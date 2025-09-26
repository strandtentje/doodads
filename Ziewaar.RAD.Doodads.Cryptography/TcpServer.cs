using System.Net;
using System.Net.Sockets;
using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;

namespace Ziewaar.RAD.Doodads.Cryptography;

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
        if (!IPEndPoint.TryParse(endpointString, out var listenEndpoint))
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Endpoint string badly formatted"));
            return;
        }

        var repeater = new RepeatInteraction(CurrentRepeatName, interaction);
        repeater.IsRunning = true;
        var ingestAnother = new Semaphore(0, 1);
        using TcpListener listener = new(listenEndpoint);
        while (repeater.IsRunning)
        {
            repeater.IsRunning = false;
            listener.BeginAcceptTcpClient(NewTcpClientHandler,
                new TcpServerClientAcquired(listener, interaction, ingestAnother));
            ingestAnother.WaitOne();
            OnThen?.Invoke(this, repeater);
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