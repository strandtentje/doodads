using MySqlConnector;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;
using Ziewaar.RAD.Doodads.SQLite;

namespace Ziewaar.RAD.Doodads.MySQL;
public class MySqlConnectionSource : ConnectionSource<MySqlConnection, MySqlCommand>
{
    private readonly UpdatingPrimaryValue ConnectionStringConst = new();
    private string? ConnectionString;
    protected override MySqlConnection CreateConnection() => new(ConnectionString);
    protected override bool IsReloadRequired(StampedMap constants, IInteraction interaction)
    {
        if ((constants, ConnectionStringConst).IsRereadRequired(out string? _connectionString) &&
            this.ConnectionString != _connectionString)
        {
            this.ConnectionString = _connectionString;
            return true;
        }
        else
        {
            return false;
        }
    }
    protected override ICommandTextPreprocessor TextPreprocessor { get; } =
        new DefaultCommandTextPreprocessor("mysql", '@');
}