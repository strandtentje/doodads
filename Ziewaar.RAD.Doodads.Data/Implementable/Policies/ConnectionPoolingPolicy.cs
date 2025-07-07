#nullable enable
using System;
using System.Data;
using Microsoft.Extensions.ObjectPool;
using Ziewaar.RAD.Doodads.Data.Implementable.Support;

namespace Ziewaar.RAD.Doodads.Data.Implementable.Policies;
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
        obj.SloppyDispose();
        return false;
        if (obj.State == ConnectionState.Open)
            return true;

        obj.SloppyDispose();
        return false;
    }
}