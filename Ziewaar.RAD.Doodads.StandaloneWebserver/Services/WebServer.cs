#pragma warning disable 67
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
            OnStarted?.Invoke(this, new CommonInteraction(interaction));
            CurrentListener.BeginGetContext(NewIncomingContext, CurrentListener);
        }
    }
    private void NewIncomingContext(IAsyncResult ar)
    {
        if (ar.AsyncState is not HttpListener servingListener ||
            servingListener != CurrentListener)
        {
            OnException?.Invoke(this,
                new CommonInteraction(
                    StartingInteraction ?? StopperInteraction.Instance,
                    "New incoming context from strange server"));
            return;
        }
        var httpContext = servingListener.EndGetContext(ar);
        CurrentListener?.BeginGetContext(NewIncomingContext, CurrentListener);
        try
        {
            var headInteraction = new HttpHeadInteraction(StartingInteraction ?? VoidInteraction.Instance, httpContext);
            OnHead?.Invoke(this, headInteraction);
            var requestInteraction = new HttpRequestInteraction(headInteraction, httpContext);
            var responseInteraction = new HttpResponseInteraction(requestInteraction, httpContext);
            OnThen?.Invoke(this, responseInteraction);
        }
#if !DEBUG
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
#endif
        finally
        {
            httpContext.Response.Close();
#if DEBUG
            if (CurrentListener == null || !CurrentListener.IsListening)
                TerminateListener(StopperInteraction.Instance);
            else
#endif
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
    private bool ValidatePrefixes(IInteraction interaction)
    {
        if (Prefixes is { } updatedPrefixesArray)
        {
            UrlAccessGuarantor.EnsureUrlAcls(Prefixes);
            CurrentListener = new();
            foreach (var item in updatedPrefixesArray)
            {
                CurrentListener.Prefixes.Add(item);
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