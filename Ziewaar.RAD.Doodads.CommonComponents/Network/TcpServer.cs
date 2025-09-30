using System.Net;
using System.Net.Sockets;
using System.Threading;
using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;

#pragma warning disable 67

namespace Ziewaar.RAD.Doodads.CommonComponents.Network;
#nullable enable
[Category("System & IO")]
[Title("Listen for TCP clients")]
[Description("""
             Provided an endpoint, a name for this server's loop, and client handling,
             start handling TCP traffic.
             """)]
public class TcpServer : IService, IDisposable
{
    [PrimarySetting("TCP Server Loop name")]
    private readonly UpdatingPrimaryValue RepeatNameConstant = new();

    private string? CurrentRepeatName;

    [EventOccasion("Provide a named Continue here to check after each new connection if we must continue listening.")]
    public event CallForInteraction? OnThen;

    [EventOccasion("Sink `address:port` string here, to listen on.")]
    public event CallForInteraction? OnIpEndpoint;

    [EventOccasion(
        "Full duplex (sourcing and sinking) client interaction here; client-ip and client-port are in memory")]
    public event CallForInteraction? OnClient;

    [NeverHappens] public event CallForInteraction? OnElse;

    [EventOccasion("Likely happens when the loop name was missing or the ip/port string was bad.")]
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, RepeatNameConstant).IsRereadRequired(out string? repeatNameCandidate))
            this.CurrentRepeatName = repeatNameCandidate;
        if (this.CurrentRepeatName == null || string.IsNullOrWhiteSpace(this.CurrentRepeatName))
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Repeat name required"));
            return;
        }

        var tsi = new TextSinkingInteraction(interaction);
        OnIpEndpoint?.Invoke(this, tsi);
        var endpointString = tsi.ReadAllText();

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
        using var ingestAnother = new Semaphore(15, 15);
        TcpListener listener = new(listenEndpoint);

        Disposing += OnDisposing;
        listener.Start();
        try
        {
            while (true)
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

        return;

        void OnDisposing(object sender, EventArgs args)
        {
            repeater.IsRunning = false;
            try
            {
                listener.Stop();
                // ReSharper disable once AccessToDisposedClosure
                ingestAnother?.Release();
                // ReSharper disable once AccessToDisposedClosure
                // ReSharper disable once DisposeOnUsingVariable
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                ingestAnother.Dispose();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            }
            catch (Exception e)
            {
                // ignored
            }
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
        }

        using (freshClient)
        {
            var clientInteraction =
                new TcpClientDuplexInteraction(asyncResult.Origin, freshClient, freshClient.GetStream());
            OnClient?.Invoke(this, clientInteraction);
            freshClient.Close();
            asyncResult.IngestAnother.Release();
        }
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
    private event EventHandler? Disposing;

    public void Dispose()
    {
        Disposing?.Invoke(this, EventArgs.Empty);
    }
}