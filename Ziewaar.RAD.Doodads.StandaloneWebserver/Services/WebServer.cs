﻿using System.Threading.Tasks.Sources;

#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services;

[Category("Http")]
[Title("Http Webserver")]
[Description("Starts listening for web requests when it receives a start command.")]
public class WebServer : IService, IDisposable
{
    private readonly PrefixProcessor Prefixes = new();
    private readonly ControlCommandInstanceProvider<ServerCommand> Server = new();
    [PrimarySetting(@"Set a whitelist array of prefixes here ie [""http://*:8008/""]")]
    private readonly UpdatingPrimaryValue PrimaryPrefixesConstant = new();
    private string[] ActivePrefixStrings = [];
    private IInteraction? StartingInteraction;
    [EventOccasion("When a request came in ready for processing")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [EventOccasion("When the prefixes weren't set up right, or when the server was DDoSed to death.")]
    public event CallForInteraction? OnException;
    [EventOccasion("Before the requst body, but after the head of the request is ready")]
    public event CallForInteraction? OnHead;
    [EventOccasion("When the server is ready to send requests to.")]
    public event CallForInteraction? OnStarted;
    [EventOccasion("When the server is no longer ready for requesting.")]
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