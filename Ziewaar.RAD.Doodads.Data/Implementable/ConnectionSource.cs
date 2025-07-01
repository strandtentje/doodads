#nullable enable
using System;
using System.Data;
using System.Threading;
using Microsoft.Extensions.ObjectPool;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

namespace Ziewaar.RAD.Doodads.SQLite;
#pragma warning disable 67
public abstract class ConnectionSource<TConnection, TCommand> : IService, IDisposable
    where TConnection : class, IDbConnection
    where TCommand : class, IDbCommand
{
    [EventOccasion("When the connection has become available")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [EventOccasion("When the current configuration could not result in a valid connection")]
    public event CallForInteraction? OnException;
    private ThreadLocal<ObjectPool<TConnection>>? CurrentConnections;
    private ThreadLocal<ObjectPool<TCommand>>? CurrentCommands;
    protected abstract TConnection CreateConnection();
    protected abstract bool IsReloadRequired(StampedMap constants, IInteraction interaction);
    protected abstract ICommandTextPreprocessor TextPreprocessor { get; }
    private void PurgeAll(IInteraction? interaction = null)
    {
        try
        {
            CurrentCommands?.Dispose();
        }
        catch (Exception ex)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction ?? StopperInteraction.Instance, ex));
        }
        try
        {
            CurrentConnections?.Dispose();
        }
        catch (Exception ex)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction ?? StopperInteraction.Instance, ex));
        }
    }
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (IsReloadRequired(constants, interaction))
        {
            this.PurgeAll(interaction);
            this.CurrentConnections = new(() =>
                new DefaultObjectPool<TConnection>(
                    new ConnectionPoolingPolicy<TConnection>(CreateConnection)));
            this.CurrentCommands = new(() =>
                new DefaultObjectPool<TCommand>(
                    new CommandPoolingPolicy<TConnection, TCommand>(this.CurrentConnections)));
        }
        if (this.CurrentCommands is ThreadLocal<ObjectPool<TCommand>> validThreadLocalCommandPool)
        {
            var commandInteraction =
                new CommandSourceInteraction<TCommand>(interaction, validThreadLocalCommandPool, TextPreprocessor);
            OnThen?.Invoke(this, commandInteraction);
        }
        else
        {
            OnException?.Invoke(this,  new CommonInteraction(interaction, "no connection was setup."));
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
    public void Dispose()
    {
        CurrentConnections?.Dispose();
        CurrentCommands?.Dispose();
    }
}