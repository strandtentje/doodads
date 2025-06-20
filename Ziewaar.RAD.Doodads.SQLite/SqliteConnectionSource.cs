#nullable enable
#pragma warning disable 67
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.ObjectPool;
using System;
using System.Threading;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

namespace Ziewaar.RAD.Doodads.SQLite;

public class SqliteConnectionSource : IService, IDisposable
{
    private readonly UpdatingPrimaryValue DataSourceFileConstant = new();

    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    private string? CurrentDataSource;
    private ThreadLocal<ObjectPool<SqliteConnection>>? CurrentConnections;
    private ThreadLocal<ObjectPool<SqliteCommand>>? CurrentCommands;
    private ObjectPool<SqliteConnection> GenerateSqliteConnectionPool() => CurrentDataSource != null ?
        new DefaultObjectPool<SqliteConnection>(new SqliteConnectionPolicy(CurrentDataSource)) :
        throw new Exception("data source required");
    private ObjectPool<SqliteCommand> GenerateSqliteCommandPool() => CurrentConnections != null ?
        new DefaultObjectPool<SqliteCommand>(new SqliteCommandPolicy(CurrentConnections)) :
        throw new Exception("no connection source");
    private void PurgeConnectionsCommands()
    {
        try
        {
            CurrentCommands?.Dispose();
        }
        catch (Exception)
        {

        }
        try
        {
            CurrentConnections?.Dispose();
        }
        catch (Exception)
        { 
            
        }
    }
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, DataSourceFileConstant).IsRereadRequired(out object? ds) && ds?.ToString() is string goodPath)
        {
            PurgeConnectionsCommands();
            this.CurrentDataSource = goodPath;
            this.CurrentConnections = new(GenerateSqliteConnectionPool);
            this.CurrentCommands = new(GenerateSqliteCommandPool);
        }
        if (this.CurrentCommands == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "no commands"));
            return;
        }
        OnThen?.Invoke(this, new SqliteCommandSourceInteraction(interaction, this.CurrentCommands));
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);

    public void Dispose() => PurgeConnectionsCommands();
}
