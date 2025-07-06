#nullable enable
using System.Data;
using System.Threading;
using Microsoft.Extensions.ObjectPool;
using Ziewaar.RAD.Doodads.Data.Implementable.Support;

namespace Ziewaar.RAD.Doodads.Data.Implementable.Policies;
#pragma warning disable 67
public class CommandPoolingPolicy<TConnection, TCommand>(ThreadLocal<ObjectPool<TConnection>> connectionPool)
    : IPooledObjectPolicy<TCommand>
    where TConnection : class, IDbConnection
    where TCommand : class, IDbCommand
{
    public TCommand Create()
    {
        var openConnection = connectionPool.Value.Get();
        return (TCommand)openConnection.CreateCommand();
    }
    public bool Return(TCommand obj)
    {
        var connection = obj.Connection;
        if (connection == null)
        {
            obj.SloppyDispose();
            return false;
        }
        if (connection.State != ConnectionState.Open)
            obj.SloppyDispose();
        if (connection.State != ConnectionState.Open)
        {
            obj.SloppyDispose();
            return false;
        }
        if (connection.State == ConnectionState.Open)
        {
            obj.CommandText = "";
            obj.Parameters.Clear();
            obj.Transaction = null;
        }
        return connection.State == ConnectionState.Open;
    }
}