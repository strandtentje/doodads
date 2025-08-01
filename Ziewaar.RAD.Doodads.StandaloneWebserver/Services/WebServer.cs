using System.Threading.Tasks.Sources;

#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services;

public class WebServer : IService, IDisposable
{
    private readonly PrefixProcessor Prefixes = new();
    private readonly ControlCommandInstanceProvider<ServerCommand> Server = new();
    private readonly UpdatingPrimaryValue PrimaryPrefixesConstant = new();
    private string[] ActivePrefixStrings = [];
    private IInteraction? StartingInteraction;
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public event CallForInteraction? OnHead;
    public event CallForInteraction? OnStarted;
    public event CallForInteraction? OnStopping;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        switch (Prefixes.TryHandlePrefixChanges(constants, PrimaryPrefixesConstant, out string[]? strings))
        {
            case PrefixProcessor.ChangeState.Empty:
                OnException?.Invoke(this, new CommonInteraction(interaction, "No prefixes were configured"));
                return;
            case PrefixProcessor.ChangeState.Changed:
                this.ActivePrefixStrings = strings!;
                Server.Reset();
                break;
            case PrefixProcessor.ChangeState.NotChanged:
            default: break;
        }

        if (Server.TryHandleCommand<ResilientHttpListenerWrapper>(interaction, ServerCommand.Stop, null,
                out var stoppable))
        {
            OnStopping?.Invoke(this, StartingInteraction ?? StopperInteraction.Instance);
            Server.Reset();
        }

        if (Server.TryHandleCommand<ResilientHttpListenerWrapper>(interaction, ServerCommand.Start,
                ListenerWrapperFactory, out var startable))
        {
            startable.Fatality += (sender, exception) =>
            {
                OnException?.Invoke(this, new CommonInteraction(interaction, exception));
            };
            startable.NewContext += (sender, context) =>
            {
                var headInteraction = new HttpHeadInteraction(StartingInteraction ?? VoidInteraction.Instance, context,
                    Prefixes.ActiveExpandedPrefixes);
                OnHead?.Invoke(this, headInteraction);
                var requestInteraction = new HttpRequestInteraction(headInteraction, context);
                var responseInteraction = new HttpResponseInteraction(requestInteraction, context);
                OnThen?.Invoke(this, responseInteraction);
            };
            this.StartingInteraction = interaction;
            startable.GiveCommand(ServerCommand.Start);
            GlobalLog.Instance?.Information("Server started {prefixes}",
                JsonConvert.SerializeObject(Prefixes.ActiveExpandedPrefixes, Formatting.Indented));
            OnStarted?.Invoke(this, StartingInteraction);
        }
    }

    private IControlCommandReceiver<ServerCommand> ListenerWrapperFactory()
    {
        return new ResilientHttpListenerWrapper(prefixes: ActivePrefixStrings, threadCount: 8);
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
    public void Dispose() => Server.Reset();
}