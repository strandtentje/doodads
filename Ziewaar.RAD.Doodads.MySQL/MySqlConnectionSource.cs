using MySqlConnector;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;
using Ziewaar.RAD.Doodads.SQLite;

namespace Ziewaar.RAD.Doodads.MySQL;

[Category("Data")]
[Title("Connect to MySQL")]
[Description("""
    Provided a MySQL connection string, connect to a server for querying. Dont forget 
    `Allow User Variables`
    """)]
public class MySqlConnectionSource : ConnectionSource<MySqlConnection, MySqlCommand>
{
    [PrimarySetting("Connection string")]
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