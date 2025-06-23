#nullable enable
using System;
using System.Data;
using Microsoft.Extensions.ObjectPool;

namespace Ziewaar.RAD.Doodads.SQLite;
#pragma warning disable 67
public class ConnectionPoolingPolicy<TConnection>(Func<TConnection> factory)
    : IPooledObjectPolicy<TConnection>
    where TConnection : class, IDbConnection
{
    public TConnection Create()
    {
        var newConnection = factory();
        if (newConnection.State != ConnectionState.Open)
            newConnection.Open();
        return newConnection;
    }
    public bool Return(TConnection obj)
    {
        if (obj.State == ConnectionState.Open)
            return true;

        obj.SloppyDispose();
        return false;
    }
}