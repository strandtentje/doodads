#nullable enable
#pragma warning disable 67
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.ObjectPool;
using System;
using System.Data;
using System.Threading;

namespace Ziewaar.RAD.Doodads.SQLite;

internal class SqliteCommandPolicy(ThreadLocal<ObjectPool<SqliteConnection>> sqliteConnections) : IPooledObjectPolicy<SqliteCommand>
{
    public SqliteCommand Create()
    {
        var freeCommand = sqliteConnections.Value.Get();
        return freeCommand.CreateCommand();
    }

    public bool Return(SqliteCommand obj)
    {
        SqliteConnection? commandConnection = obj.Connection;
        if (commandConnection == null)
        {
            try
            {
                obj.Dispose();
            }
            catch (Exception)
            {
                // whatever
            }
            return false;
        }
        if (commandConnection.State != ConnectionState.Open)
        {
            try
            {
                obj.Dispose();
            }
            catch (Exception)
            {

            }
        }
        if (commandConnection.State != ConnectionState.Open)
        {
            try
            {
                obj.Dispose();
            }
            catch (Exception)
            {

            }
            return false;
        }
        if (commandConnection.State == ConnectionState.Open)
        {
            obj.CommandText = "";
            obj.Parameters.Clear();
            obj.Transaction = null;
            return true;
        }
        return false;
    }
}