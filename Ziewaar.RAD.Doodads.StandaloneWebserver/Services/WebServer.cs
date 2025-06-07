namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services;

public class WebServer : IService, IDisposable
{
    public event EventHandler<IInteraction>? OnError;
    public event EventHandler<IInteraction>? PrefixesRequested;
    [DefaultBranch]
    public event EventHandler<IInteraction>? HandleRequest;
    private HttpListener? CurrentListener = null;
    private long LastUpdateFromBranch;
    private long LastUpdateFromConstants;
    private string[]? Prefixes;
    private IInteraction? StartingInteraction;

    public void Enter(ServiceConstants serviceConstants, IInteraction interaction)
    {
        HandleStopCommand(interaction);
        UpdatePrefixes(serviceConstants, interaction);

        if (ValidateStartCommand(interaction) && ValidatePrefixes(interaction))
        {
            StartingInteraction = interaction;
            CurrentListener!.Start();
            CurrentListener.BeginGetContext(NewIncomingContext, CurrentListener);
        }
    }

    private void NewIncomingContext(IAsyncResult ar)
    {
        if (ar.AsyncState is not HttpListener servingListener ||
            servingListener != CurrentListener)
        {
            OnError?.Invoke(this,
                VariablesInteraction.ForError(
                    StartingInteraction,
                    "New incoming context from strange server"));
            return;
        }
        var httpContext = servingListener.EndGetContext(ar);
        CurrentListener?.BeginGetContext(NewIncomingContext, CurrentListener);
        var httpInteraction = new HttpInteraction(StartingInteraction ?? VoidInteraction.Instance, httpContext);
        HandleRequest?.Invoke(this, httpInteraction);
        httpContext.Response.Close();
    }

    private void HandleStopCommand(IInteraction interaction)
    {
        if (interaction.TryGetClosest<ServerCommandInteraction>(
            out var stopper,
            stopper => stopper.Command == ServerCommand.Stop) &&
            CurrentListener != null && CurrentListener.IsListening)
        {
            stopper!.Consume();
            CurrentListener.Stop();
            CurrentListener = null;
            StartingInteraction = null;
        }
    }

    private void UpdatePrefixes(ServiceConstants serviceConstants, IInteraction interaction)
    {
        var prefixSink =
            CurrentListener == null ?
            new PrefixSinkingInteraction(interaction, 0, SidechannelState.Always) :
            new PrefixSinkingInteraction(interaction, LastUpdateFromBranch, SidechannelState.StampGreater);

        PrefixesRequested?.Invoke(this, prefixSink);

        Prefixes =
            prefixSink.GetAllPrefixes(ref LastUpdateFromBranch) ??
            serviceConstants.RequireUpdatedItemsOf<string>("prefixes", ref LastUpdateFromConstants) ??
            Prefixes;        
    }

    private bool ValidateStartCommand(IInteraction interaction)
    {
        if (!interaction.TryGetClosest<ServerCommandInteraction>(out var starter,
            starter => starter.Command == ServerCommand.Start))
            return false;

        starter.Consume();

        if (CurrentListener != null && CurrentListener.IsListening)
        {
            OnError?.Invoke(this, VariablesInteraction.ForError(interaction, "Server already started"));
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
        if (Prefixes is string[] updatedPrefixesArray)
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
            OnError?.Invoke(this, VariablesInteraction.ForError(interaction, "No Prefix Provided and No Server Running"));
            return false;
        }
    }

    public void Dispose() => CurrentListener?.Close();
}