namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services;
#nullable enable
public class WebServer : IService, IDisposable
{
    private HttpListener? CurrentListener = null;
    private long LastUpdateFromBranch;
    private long LastUpdateFromConstants;
    private string[]? Prefixes;
    private IInteraction? StartingInteraction;
    public event EventHandler<IInteraction>? OnThen;
    public event EventHandler<IInteraction>? OnElse;
    public event EventHandler<IInteraction>? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        HandleStopCommand(interaction);
        UpdatePrefixes(constants, interaction);

        if (Prefixes.Length == 0)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "no prefixes were configured"));
            return;
        }

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
            OnException?.Invoke(this,
                new CommonInteraction(
                    StartingInteraction ?? StopperInteraction.Instance,
                    "New incoming context from strange server"));
            return;
        }

        var httpContext = servingListener.EndGetContext(ar);
        CurrentListener?.BeginGetContext(NewIncomingContext, CurrentListener);
        var httpInteraction = new HttpInteraction(StartingInteraction ?? VoidInteraction.Instance, httpContext);
        OnThen?.Invoke(this, httpInteraction);
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
    private readonly UpdatingPrimaryValue PrefixesConstant = new();
    private void UpdatePrefixes(StampedMap serviceConstants, IInteraction interaction)
    {
        (serviceConstants, PrefixesConstant).IsRereadRequired(out object[]? prefixObjArr);
        Prefixes = prefixObjArr?.OfType<string>().ToArray();

        if (Prefixes == null || Prefixes.Length == 0)
        {
            var tsi = new TextSinkingInteraction(interaction, delimiter: "\n");
            OnElse?.Invoke(this, tsi);
            using var rdr = tsi.GetDisposingSinkReader();
            var newList = new List<string>();
            while (!rdr.EndOfStream && rdr.ReadLine() is { } line)
                newList.Add(line);
            Prefixes = newList.ToArray();
        }
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
    public void Dispose() => CurrentListener?.Close();
}