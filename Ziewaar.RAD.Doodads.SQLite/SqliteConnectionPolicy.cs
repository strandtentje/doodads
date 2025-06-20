#nullable enable
#pragma warning disable 67
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.ObjectPool;
using System.Data;

namespace Ziewaar.RAD.Doodads.SQLite;

internal class SqliteConnectionPolicy(string filename) : IPooledObjectPolicy<SqliteConnection>
{
    public SqliteConnection Create()
    {
        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = filename
        };
        var newConnection =  new SqliteConnection(builder.ToString());
        newConnection.Open();
        return newConnection;
    }

    public bool Return(SqliteConnection obj)
    {
        if (obj.State == ConnectionState.Open)
        {
            return true;
        }
        else
        {
            obj.Dispose();
            return false;
        }
    }
}
