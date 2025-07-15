#pragma warning disable 67
using System.Net.Sockets;

namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services;

public class WebServer : IService, IDisposable
{
    private HttpListener? CurrentListener = null;
    private string[]? Prefixes;
    private IInteraction? StartingInteraction;
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public event CallForInteraction? OnHead;
    public event CallForInteraction? OnStarted;
    public event CallForInteraction? OnStopping;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        HandleStopCommand(interaction);
        UpdatePrefixes(constants, interaction);

        if (Prefixes == null || Prefixes.Length == 0)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "no prefixes were configured"));
            return;
        }

        if (ValidateStartCommand(interaction) && ValidatePrefixes(interaction))
        {
            StartingInteraction = interaction;
            CurrentListener!.Start();
            OnStarted?.Invoke(this, new CommonInteraction(interaction, memory: new SwitchingDictionary(["localhosturl"], x => x switch
            {
                "localhosturl" => ExpandedPrefixes.LocalIPURL,
                _ => throw new KeyNotFoundException(),
            })));
            CurrentListener.BeginGetContext(NewIncomingContext, CurrentListener);
        }
    }
    private void NewIncomingContext(IAsyncResult ar)
    {
        if (isTerminating)
        {
            Console.WriteLine("Aborting Request...");
            return;
        }
        if (ar.AsyncState is not HttpListener servingListener ||
            servingListener != CurrentListener)
        {
            OnException?.Invoke(this,
                new CommonInteraction(
                    StartingInteraction ?? StopperInteraction.Instance,
                    "New incoming context from strange server"));
            return;
        }
        HttpListenerContext? httpContext = null;
        try
        {
            httpContext = servingListener.EndGetContext(ar);
            CurrentListener?.BeginGetContext(NewIncomingContext, CurrentListener);
            var headInteraction = new HttpHeadInteraction(StartingInteraction ?? VoidInteraction.Instance, httpContext, ExpandedPrefixes);
            OnHead?.Invoke(this, headInteraction);
            var requestInteraction = new HttpRequestInteraction(headInteraction, httpContext);
            var responseInteraction = new HttpResponseInteraction(requestInteraction, httpContext);
            OnThen?.Invoke(this, responseInteraction);
        }
        catch (Exception ex)
        {
            var exceptionalInteraction =
                new CommonInteraction(StartingInteraction ?? StopperInteraction.Instance, ex.Message);
            OnException?.Invoke(this, exceptionalInteraction);
            if (CurrentListener == null || !CurrentListener.IsListening)
                TerminateListener(exceptionalInteraction);
            else
                CurrentListener?.BeginGetContext(NewIncomingContext, CurrentListener);
        }
        finally
        {
            try
            {
                httpContext?.Response.Close();
            }
            catch (Exception)
            {
            }
            if (CurrentListener == null || !CurrentListener.IsListening)
                TerminateListener(StopperInteraction.Instance);
            else
                CurrentListener?.BeginGetContext(NewIncomingContext, CurrentListener);
        }
    }
    private void HandleStopCommand(IInteraction interaction)
    {
        if (interaction.TryGetClosest<ServerCommandInteraction>(
                out var stopper,
                stopper => stopper.Command == ServerCommand.Stop) &&
            CurrentListener != null && CurrentListener.IsListening)
        {
            stopper!.Consume();
            TerminateListener(interaction);
        }
    }
    private readonly object terminationLock = new();
    private bool isTerminating = false;
    private void TerminateListener(IInteraction interaction)
    {
        if (isTerminating) return;
        lock (terminationLock)
        {
            if (isTerminating) return;
            if (CurrentListener == null)
            {
                return;
            }
            isTerminating = true;
            OnStopping?.Invoke(this, interaction);
            try
            {
                CurrentListener?.Stop();
            }
            catch (Exception)
            {
                // its already broken
            }
            try
            {
                CurrentListener?.Close();
            }
            catch (Exception)
            {
                // its already broken
            }
            CurrentListener = null;
            StartingInteraction = null;
            isTerminating = false;
        }
    }
    private readonly UpdatingPrimaryValue PrefixesConstant = new();
    private void UpdatePrefixes(StampedMap serviceConstants, IInteraction interaction)
    {
        (serviceConstants, PrefixesConstant).IsRereadRequired(out object[]? prefixObjArr);
        Prefixes = prefixObjArr?.OfType<string>().ToArray();

        if (Prefixes == null || Prefixes.Length == 0)
            OnException?.Invoke(this,
                new CommonInteraction(interaction, "no array of prefixes configured for the WebServer"));
    }
    private bool ValidateStartCommand(IInteraction interaction)
    {
        if (!interaction.TryGetClosest<ServerCommandInteraction>(out var starter,
                s => s.Command == ServerCommand.Start) ||
            starter == null)
            return false;

        starter.Consume();

        if (CurrentListener != null && CurrentListener.IsListening)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Server already started"));
            return false;
        }
        else
        {
            CurrentListener = new();
            return true;
        }
    }
    ExpandedPrefixes ExpandedPrefixes = new();
    public static bool TryGetLocalIPAddress(out string addr)
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                addr = ip.ToString();
                return true;
            }
        }
        addr = "";
        return false;
    }

    private bool ValidatePrefixes(IInteraction interaction)
    {
        if (Prefixes is { } updatedPrefixesArray)
        {
            UrlAccessGuarantor.EnsureUrlAcls(Prefixes);
            CurrentListener = new();
            ExpandedPrefixes = new();
            foreach (var item in updatedPrefixesArray)
            {
                CurrentListener.Prefixes.Add(item);
                ExpandedPrefixes.LoopbackURL = item.Replace("*", "127.0.0.1").Replace("+", "127.0.0.1");
                if (TryGetLocalIPAddress(out string addr))
                    ExpandedPrefixes.LocalIPURL = item.Replace("*", addr).Replace("+", addr);
                try
                {
                    var hostname = Dns.GetHostName();
                    ExpandedPrefixes.LocalHostnameURL = item.Replace("*", hostname).Replace("+", hostname);
                } catch(Exception)
                {
                    // whatever
                }
            }

            return true;
        }
        else
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "No Prefix Provided and No Server Running"));
            return false;
        }
    }
    public void Dispose() => TerminateListener(StopperInteraction.Instance);
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}