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
/*
        // We've unpooled the pool of connection pools here.
        // In high volume situations bringing the pooling into
        // .net and making IDbConnections long-lived, does make a 
        // very measurable impact. Same for IDataCommand.
        // However, not every IDbConnection implementation likes 
        // this. So we switch it off for now until we can find a 
        // way to switch this on only when it  won't cause problems.
        
        if (obj.State == ConnectionState.Open)
            return true;

        obj.SloppyDispose();
        return false;
*/
    }
}